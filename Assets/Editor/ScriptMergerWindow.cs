using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ScriptMergerWindow : EditorWindow
{
    private DefaultAsset selectedFolder;
    private bool includeSubdirectories = true;
    private bool filterComments = false; 
    private string fileExtensions = "*.cs;*.shader;*.compute";

    [MenuItem("Tools/导出脚本内容到 TXT (Script Merger)")]
    public static void ShowWindow()
    {
        GetWindow<ScriptMergerWindow>("Script Merger");
    }

    private void OnGUI()
    {
        GUILayout.Label("应付Alan走路人的导出脚本工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. 文件夹选择
        GUILayout.Label("1. 选择源文件夹:", EditorStyles.label);
        selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "目标文件夹",
            selectedFolder,
            typeof(DefaultAsset),
            false
        );

        // 自动获取选中
        if (selectedFolder == null && Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(path))
            {
                selectedFolder = Selection.activeObject as DefaultAsset;
            }
        }

        EditorGUILayout.Space();

        GUILayout.Label("2. 配置选项:", EditorStyles.label);
        includeSubdirectories = EditorGUILayout.Toggle("包含子文件夹", includeSubdirectories);

        filterComments = EditorGUILayout.Toggle(new GUIContent("过滤代码注释", "勾选后将自动删除 // 和 /* */ 注释，并清理空行"), filterComments);
   

        fileExtensions = EditorGUILayout.TextField("文件后缀 (分号隔开)", fileExtensions);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.enabled = selectedFolder != null;
        if (GUILayout.Button("选择保存位置并导出", GUILayout.Height(40)))
        {
            ExportScripts();
        }
        GUI.enabled = true;

        if (selectedFolder == null)
        {
            EditorGUILayout.HelpBox("请先选择一个文件夹。", MessageType.Info);
        }
    }

    private void ExportScripts()
    {
        string folderPath = AssetDatabase.GetAssetPath(selectedFolder);
        string fullFolderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", folderPath));

        string savePath = EditorUtility.SaveFilePanel("保存合并后的文本文件", "", selectedFolder.name + (filterComments ? "_NoComments" : "") + "_Scripts", "txt");

        if (string.IsNullOrEmpty(savePath)) return;

        string[] extensions = fileExtensions.Split(';');
        List<string> filesToRead = new List<string>();

        SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var ext in extensions)
        {
            try
            {
                string[] files = Directory.GetFiles(fullFolderPath, ext, searchOption);
                filesToRead.AddRange(files);
            }
            catch (System.Exception e) { Debug.LogError(e.Message); }
        }

        if (filesToRead.Count == 0) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// 导出时间: {System.DateTime.Now}");
        sb.AppendLine($"// 过滤注释: {filterComments}");
        sb.AppendLine("==================================================================");
        sb.AppendLine();

        int currentCount = 0;
        foreach (string filePath in filesToRead)
        {
            currentCount++;
            string fileName = Path.GetFileName(filePath);

            if (EditorUtility.DisplayCancelableProgressBar("正在导出", $"处理: {fileName}", (float)currentCount / filesToRead.Count))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                if (filterComments)
                {
                    content = StripComments(content);
                }

                sb.AppendLine($"// ========================================================");
                sb.AppendLine($"// 文件名: {fileName}");
                sb.AppendLine($"// 路径: {filePath.Replace(fullFolderPath, "")}");
                sb.AppendLine($"// ========================================================");
                sb.AppendLine();
                sb.AppendLine(content.Trim());
                sb.AppendLine();
                sb.AppendLine();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"读取失败 {fileName}: {e.Message}");
            }
        }

        File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
        EditorUtility.ClearProgressBar();
        System.Diagnostics.Process.Start(savePath);
    }

    /// <summary>
    /// 使用正则去除C风格的注释，同时保留字符串内的双斜杠
    /// </summary>
    private string StripComments(string input)
    {

        var blockComments = @"/\*(.*?)\*/";
        var lineComments = @"//(.*?)\r?\n";
        var strings = @"""((\\[^\n]|[^""\n])*)""";
        var verbatimStrings = @"@(""[^""]*"")+";

        string pattern = $"{verbatimStrings}|{strings}|{blockComments}|{lineComments}";

        string noComments = Regex.Replace(input, pattern, me => {
            if (me.Value.StartsWith("/"))
                return me.Value.StartsWith("//") ? System.Environment.NewLine : "";
            return me.Value;
        }, RegexOptions.Singleline);

        string result = Regex.Replace(noComments, @"(\r\n|\r|\n){3,}", "\n\n");

        return result;
    }
}
