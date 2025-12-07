using UnityEngine;
using System.Text; // 必须引用
using System.Runtime.InteropServices; // 必须引用
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputDebuger : MonoBehaviour
{
    // --- 引入 JS 函数 ---
    [DllImport("__Internal")]
    private static extern void DownloadFile(string content, string filename);

    // 日志缓存
    private StringBuilder _logHistory = new StringBuilder();
    private Vector2 _lastDelta;
    private float _maxDeltaMagnitude = 0f;
    private string _uiMessage = "Waiting...";

    public float detectionThreshold = 100f;

    void Start()
    {
        // 初始化日志头
        _logHistory.AppendLine($"--- WebGL Input Log Start: {System.DateTime.Now} ---");
        _logHistory.AppendLine($"Screen Resolution: {Screen.width}x{Screen.height}");
        _logHistory.AppendLine($"DPR (Device Pixel Ratio): {Screen.dpi}");
        _logHistory.AppendLine("--------------------------------------------------");
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 rawDelta = Mouse.current.delta.ReadValue();

            // 只有当发生异常跳动时，才记录进文件（避免文件无限膨胀）
            // 或者你可以每隔几秒记录一次关键数据
            if (rawDelta.magnitude > detectionThreshold)
            {
                string logEntry = $"[JUMP] Time:{Time.time:F2} | Frame:{Time.frameCount} | Delta:{rawDelta} | Mag:{rawDelta.magnitude} | dt:{Time.deltaTime}";

                // 1. 存入内存缓存
                _logHistory.AppendLine(logEntry);

                // 2. 更新 UI 显示
                _uiMessage = logEntry;

                // 3. 同时发到浏览器控制台备用
                Debug.LogError(logEntry);
            }

            // 更新最大值统计
            if (rawDelta.magnitude > _maxDeltaMagnitude)
            {
                _maxDeltaMagnitude = rawDelta.magnitude;
            }
        }
#endif
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 20;
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(10, 10, 500, 300), GUI.skin.box);
        GUILayout.Label("--- INPUT RECORDER ---");
        GUILayout.Label($"Last Jump: {_uiMessage}");
        GUILayout.Label($"Max Delta Ever: {_maxDeltaMagnitude}");

        GUILayout.Space(20);

        // --- 导出按钮 ---
        if (GUILayout.Button("DOWNLOAD LOG FILE", GUILayout.Height(50)))
        {
            SaveLogToFile();
        }

        if (GUILayout.Button("Clear Logs"))
        {
            _logHistory.Clear();
            _logHistory.AppendLine($"--- Log Cleared at {Time.time} ---");
            _maxDeltaMagnitude = 0;
        }

        GUILayout.EndArea();
    }

    public void SaveLogToFile()
    {
        string content = _logHistory.ToString();
        string fileName = $"InputLog{System.DateTime.Now:MMdd_HHmm}.txt";

#if UNITY_WEBGL && !UNITY_EDITOR
        // 调用 JS 下载
        DownloadFile(content, fileName);
#else
        // 如果在编辑器里测试，直接打印出来提示
        Debug.Log("Editor Mode: logs would be downloaded in WebGL.\n" + content);
        // 你也可以在这里写 System.IO.File.WriteAllText... 方便编辑器测试
#endif
    }
}