using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Text;

public class TestSTT : MonoBehaviour
{
    private Process whisperProcess;
    private StreamWriter pythonWriter;

    public string exeFileName = "STTworker.exe"; // 빌드한 EXE 이름
    public string testFileName = "sample.wav"; //테스트 용 오디오 파일

    void Start()
    {
        StartWhisperProcess();
    }

    void Update()
    {
        // T 키를 누르면 테스트 실행
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunTest();
        }
    }

    void RunTest()
    {
        // StreamingAssets에 있는 샘플 파일 경로 가져오기
        string filePath = Path.Combine(Application.streamingAssetsPath, testFileName);

        if (File.Exists(filePath))
        {
            UnityEngine.Debug.Log($"[테스트 시작] 파일 경로: {filePath}");
            RequestTranscribe(filePath);
        }
        else
        {
            UnityEngine.Debug.LogError($"[테스트 실패] 파일을 찾을 수 없습니다: {filePath}");
            UnityEngine.Debug.Log("팁: Assets/StreamingAssets 폴더 안에 sample.wav 파일을 넣어주세요.");
        }
    }

    void StartWhisperProcess()
    {
        // 파이썬 EXE 경로
        string exePath = Path.Combine(Application.streamingAssetsPath, "STTworker", exeFileName);

        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("EXE 파일을 찾을 수 없습니다. 경로를 확인하세요: " + exePath);
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true, // 이것이 true인지 확인!
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        whisperProcess = new Process { StartInfo = startInfo };

        // 파이썬의 출력을 실시간으로 가로챔
        whisperProcess.OutputDataReceived += (sender, e) => {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // 파이썬에서 READY 또는 STT 결과가 올 때 출력됨
                UnityEngine.Debug.Log($"<color=cyan>[Whisper 서버 응답]</color> {e.Data}");
            }
        };

        whisperProcess.Start();
        whisperProcess.BeginOutputReadLine();
        pythonWriter = whisperProcess.StandardInput;

        UnityEngine.Debug.Log("Whisper 프로세스가 시작되었습니다. 'READY' 메시지를 기다리세요.");
    }

    public void RequestTranscribe(string filePath)
    {
        if (pythonWriter == null) return;

        // JSON 규격 생성 (파이썬이 인식할 수 있게)
        string formattedPath = filePath.Replace("\\", "/");
        string jsonRequest = $"{{\"cmd\": \"transcribe\", \"file\": \"{formattedPath}\"}}";

        pythonWriter.WriteLine(jsonRequest);
        pythonWriter.Flush();
    }

    void OnApplicationQuit()
    {
        if (whisperProcess != null && !whisperProcess.HasExited)
        {
            whisperProcess.Kill();
        }
    }
}