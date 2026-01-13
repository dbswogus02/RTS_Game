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
        // 마이크 이름, 루프여부, 최대시간, 샘플레이트(16000 권장)
        recordingClip = Microphone.Start(null, false, maxRecordTime, 16000);
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
            // 1. WAV 파일로 저장
            SaveWavFile(recordingClip, lastPos);
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
    // AudioClip을 WAV 파일 포맷으로 변환하여 저장
    void SaveWavFile(AudioClip clip, int lengthSamples)
    {
        using (var fileStream = new FileStream(tempWavPath, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            var samples = new float[lengthSamples * clip.channels];
            clip.GetData(samples, 0);

            // WAV 헤더 작성
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + samples.Length * 2);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((ushort)(clip.channels * 2));
            writer.Write((ushort)16);
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(samples.Length * 2);

            // 데이터 작성 (float -> short 변환)
            foreach (var sample in samples)
            {
                writer.Write((short)(sample * short.MaxValue));
            }
        }
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