using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Collections;

public class STTManage : MonoBehaviour
{
    [Header("Settings")]
    public string exeFileName = "STTworker.exe"; // 빌드한 EXE 이름
    public int maxRecordTime = 30; //최대 시간

    private Process whisperProcess;
    private StreamWriter pythonWriter;
    private string tempWavPath; //녹음 파일 임시 저장소

    private AudioClip recordingClip;
    private bool isRecording = false;

    void Start()
    {
        // 임시 파일 경로 설정
        tempWavPath = Path.Combine(Application.temporaryCachePath, "temp_speech.wav");
        StartWhisperProcess();
    }

    void Update()
    {
        // V 키를 누르는 동안 녹음 시작
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartRecording();
        }

        // V 키를 떼면 녹음 종료 및 STT 요청
        if (Input.GetKeyUp(KeyCode.V))
        {
            StopRecordingAndTranscribe();
        }
    }

    #region Recording Logic
    void StartRecording()
    {
        if (isRecording) return;

        UnityEngine.Debug.Log("녹음 시작...");
        isRecording = true;

        
        int minFreq;
        int maxFreq;
        Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

        UnityEngine.Debug.Log($"Mic freq range: {minFreq} ~ {maxFreq}");
        int freq = (minFreq == 0 && maxFreq == 0) ? 44100 : maxFreq;

        // 마이크 이름, 루프여부, 최대시간, 샘플레이트(16000 권장)
        recordingClip = Microphone.Start(null, false, maxRecordTime, freq);
    }

    void StopRecordingAndTranscribe()
    {
        if (!isRecording) return;

        UnityEngine.Debug.Log("녹음 종료 및 저장 중...");
        int lastPos = Microphone.GetPosition(null);
        Microphone.End(null);
        isRecording = false;

        if (lastPos > 0)
        {
            // 실제 녹음된 AudioClip 추출
            AudioClip trimmedClip = TrimClip(recordingClip, lastPos);

            // Whisper용 16kHz WAV 저장
            SaveWhisperWav(trimmedClip, tempWavPath);

            // 2. 파이썬에 STT 요청
            RequestTranscribe(tempWavPath);
        }
    }
    #endregion

    #region Communication Logic
    void StartWhisperProcess()
    {
        // EXE 경로는 StreamingAssets 폴더 내로 설정
        string exePath = Path.Combine(Application.streamingAssetsPath, "STTworker",exeFileName);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,         // 필수: false여야 창 숨기기가 작동함
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,    // 에러 출력도 리다이렉트
            CreateNoWindow = true,           // 창을 생성하지 않음
            WindowStyle = ProcessWindowStyle.Hidden, // 창 스타일을 숨김으로 설정
            StandardOutputEncoding = Encoding.UTF8
        };

        // whisperProcess가 실행됐을때 설정
        whisperProcess = new Process { StartInfo = startInfo };
        whisperProcess.OutputDataReceived += (sender, e) => {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log($"[Whisper 결과]: {e.Data}");
            }
        };

        whisperProcess.Start();
        whisperProcess.BeginOutputReadLine();
        pythonWriter = whisperProcess.StandardInput;
    }

    public void RequestTranscribe(string filePath)
    {
        // 파이썬 코드의 JSON 규격에 맞춤
        string formattedPath = filePath.Replace("\\", "/");
        string jsonRequest = $"{{\"cmd\": \"transcribe\", \"file\": \"{formattedPath}\"}}";

        pythonWriter.WriteLine(jsonRequest);
        pythonWriter.Flush();
    }
    #endregion

    #region WAV Saver (Helper)
    void WriteWavFile(string path, byte[] pcmData, int sampleRate, int channels)
    {
        using (FileStream fs = new FileStream(path, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            int byteRate = sampleRate * channels * 2;

            // RIFF
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + pcmData.Length);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1); // PCM
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write((short)(channels * 2));
            bw.Write((short)16);

            // data
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(pcmData.Length);
            bw.Write(pcmData);
        }
    }
    #endregion

    #region TrimClip (Helper)
    AudioClip TrimClip(AudioClip clip, int samples)
    {
        float[] data = new float[samples * clip.channels];
        clip.GetData(data, 0);

        AudioClip newClip = AudioClip.Create(
            "TrimmedClip",
            samples,
            clip.channels,
            clip.frequency,
            false
        );

        newClip.SetData(data, 0);
        return newClip;
    }
    #endregion

    #region 16Hz change (helper)
    void SaveWhisperWav(AudioClip clip, string path)
    {
        // 1. Mono
        float[] mono = GetMonoSamples(clip);

        // 2. Resample → 16kHz
        float[] mono16k = ResampleTo16k(mono, clip.frequency);

        // 3. PCM16 변환
        byte[] pcm16 = FloatToPCM16(mono16k);

        // 4. WAV 헤더 + 저장
        WriteWavFile(path, pcm16, 16000, 1);
    }

    public static float[] GetMonoSamples(AudioClip clip)
    {
        int channels = clip.channels;
        int samples = clip.samples;

        float[] data = new float[samples * channels];
        clip.GetData(data, 0);

        // 이미 mono
        if (channels == 1)
            return data;

        // Stereo → Mono (평균)
        float[] mono = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float sum = 0f;
            for (int c = 0; c < channels; c++)
            {
                sum += data[i * channels + c];
            }
            mono[i] = sum / channels;
        }

        return mono;
    }

    public static float[] ResampleTo16k(float[] input, int inputRate)
    {
        const int targetRate = 16000;

        if (inputRate == targetRate)
            return input;

        float ratio = (float)targetRate / inputRate;
        int outputLength = Mathf.RoundToInt(input.Length * ratio);

        float[] output = new float[outputLength];

        for (int i = 0; i < outputLength; i++)
        {
            float srcIndex = i / ratio;
            int index0 = Mathf.FloorToInt(srcIndex);
            int index1 = Mathf.Min(index0 + 1, input.Length - 1);
            float t = srcIndex - index0;

            output[i] = Mathf.Lerp(input[index0], input[index1], t);
        }

        return output;
    }

    public static byte[] FloatToPCM16(float[] samples)
    {
        byte[] pcm = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            float clamped = Mathf.Clamp(samples[i], -1f, 1f);
            short value = (short)(clamped * short.MaxValue);

            pcm[i * 2] = (byte)(value & 0xff);
            pcm[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        return pcm;
    }

    #endregion

    void OnApplicationQuit()
    {
        //파이썬 프로그램 종료
        if (whisperProcess != null && !whisperProcess.HasExited)
        {
            whisperProcess.Kill();
        }
        // 임시 녹음 파일 삭제
        if (File.Exists(tempWavPath))
        {
            File.Delete(tempWavPath);
            UnityEngine.Debug.Log("임시 파일이 삭제되었습니다.");
        }
    }
}