using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using SFB; // StandaloneFileBrowser
using System.IO;
using System;

public class GameLauncherSystem : MonoBehaviour
{
    [SerializeField] private List<string> relativeGamePaths = new List<string>(10);
    public int currentIndex = 0;
    [SerializeField] private GameObject targetButton2;

    public void SetLaunchIndex(int index)
    {
        if (index >= 0 && index < relativeGamePaths.Count)
        {
            currentIndex = index;
            UnityEngine.Debug.Log($"選択インデックス: {index}");

            targetButton2.SetActive(currentIndex != 0);
        }
        else
        {
            UnityEngine.Debug.LogError($"無効なインデックス: {index}");
        }
    }

    public void ExecuteCurrentGame()
    {
        if (currentIndex == -1)
        {
            UnityEngine.Debug.LogWarning("インデックスが設定されていません");
            return;
        }

        if (string.IsNullOrEmpty(relativeGamePaths[currentIndex]))
        {
            UnityEngine.Debug.LogWarning($"パス未設定 [{currentIndex}]");
            return;
        }

        // 実行ファイルのあるディレクトリを基準に相対パスを作成
        string basePath = Application.dataPath;

        // Windows の場合、Dataフォルダの1つ上が実行ファイルのある場所
#if UNITY_STANDALONE_WIN
        basePath = Path.GetDirectoryName(Application.dataPath); // MyApp_Data の親フォルダ
#endif

        string fullPath = Path.Combine(basePath, relativeGamePaths[currentIndex]);

        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogError($"ファイルが存在しません: {fullPath}");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = fullPath,
                UseShellExecute = true
            });
            UnityEngine.Debug.Log($"起動成功: {fullPath}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"起動失敗: {e.Message}");
        }
    }

    public void SelectGamePath(int index)
    {
        var extensions = new[] {
            new ExtensionFilter("実行可能ファイル", "exe")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("実行ファイルを選択", "", extensions, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            // 実行ファイルのルートからの相対パスに変換
            string basePath = Path.GetDirectoryName(Application.dataPath);
            string relativePath = GetRelativePath(paths[0], basePath);

            if (index >= 0 && index < relativeGamePaths.Count)
            {
                relativeGamePaths[index] = relativePath;
                UnityEngine.Debug.Log($"相対パス設定 [{index}]: {relativeGamePaths[index]}");

                // パスをセーブ
                SaveRelativeGamePaths();
            }
            else
            {
                UnityEngine.Debug.LogError($"無効なインデックス: {index}");
            }
        }
    }

    // 絶対パス → 相対パス変換
    private string GetRelativePath(string fullPath, string basePath)
    {
        Uri pathUri = new Uri(fullPath);
        Uri baseUri = new Uri(basePath + Path.DirectorySeparatorChar);
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// relativeGamePathsをセーブする
    /// </summary>
    public void SaveRelativeGamePaths()
    {
        try
        {
            ES3.Save("relativeGamePaths", relativeGamePaths);
            UnityEngine.Debug.Log("relativeGamePathsをセーブしました！");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("relativeGamePathsのセーブに失敗しました: " + ex.Message);
        }
    }

    /// <summary>
    /// relativeGamePathsをロードする
    /// </summary>
    public void LoadRelativeGamePaths()
    {
        try
        {
            if (ES3.KeyExists("relativeGamePaths"))
            {
                relativeGamePaths = ES3.Load<List<string>>("relativeGamePaths");
                UnityEngine.Debug.Log("relativeGamePathsをロードしました！");
            }
            else
            {
                UnityEngine.Debug.Log("保存されたrelativeGamePathsが見つかりませんでした。");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("relativeGamePathsのロードに失敗しました: " + ex.Message);
        }
    }

    private void Start()
    {
        // ゲーム開始時にロード
        LoadRelativeGamePaths();
    }

    private void OnApplicationQuit()
    {
        // ゲーム終了時にセーブ
        SaveRelativeGamePaths();
    }
}
