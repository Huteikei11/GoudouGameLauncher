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
            UnityEngine.Debug.Log($"�I���C���f�b�N�X: {index}");

            targetButton2.SetActive(currentIndex != 0);
        }
        else
        {
            UnityEngine.Debug.LogError($"�����ȃC���f�b�N�X: {index}");
        }
    }

    public void ExecuteCurrentGame()
    {
        if (currentIndex == -1)
        {
            UnityEngine.Debug.LogWarning("�C���f�b�N�X���ݒ肳��Ă��܂���");
            return;
        }

        if (string.IsNullOrEmpty(relativeGamePaths[currentIndex]))
        {
            UnityEngine.Debug.LogWarning($"�p�X���ݒ� [{currentIndex}]");
            return;
        }

        // ���s�t�@�C���̂���f�B���N�g������ɑ��΃p�X���쐬
        string basePath = Application.dataPath;

        // Windows �̏ꍇ�AData�t�H���_��1�オ���s�t�@�C���̂���ꏊ
#if UNITY_STANDALONE_WIN
        basePath = Path.GetDirectoryName(Application.dataPath); // MyApp_Data �̐e�t�H���_
#endif

        string fullPath = Path.Combine(basePath, relativeGamePaths[currentIndex]);

        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogError($"�t�@�C�������݂��܂���: {fullPath}");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = fullPath,
                UseShellExecute = true
            });
            UnityEngine.Debug.Log($"�N������: {fullPath}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"�N�����s: {e.Message}");
        }
    }

    public void SelectGamePath(int index)
    {
        var extensions = new[] {
            new ExtensionFilter("���s�\�t�@�C��", "exe")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("���s�t�@�C����I��", "", extensions, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            // ���s�t�@�C���̃��[�g����̑��΃p�X�ɕϊ�
            string basePath = Path.GetDirectoryName(Application.dataPath);
            string relativePath = GetRelativePath(paths[0], basePath);

            if (index >= 0 && index < relativeGamePaths.Count)
            {
                relativeGamePaths[index] = relativePath;
                UnityEngine.Debug.Log($"���΃p�X�ݒ� [{index}]: {relativeGamePaths[index]}");

                // �p�X���Z�[�u
                SaveRelativeGamePaths();
            }
            else
            {
                UnityEngine.Debug.LogError($"�����ȃC���f�b�N�X: {index}");
            }
        }
    }

    // ��΃p�X �� ���΃p�X�ϊ�
    private string GetRelativePath(string fullPath, string basePath)
    {
        Uri pathUri = new Uri(fullPath);
        Uri baseUri = new Uri(basePath + Path.DirectorySeparatorChar);
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// relativeGamePaths���Z�[�u����
    /// </summary>
    public void SaveRelativeGamePaths()
    {
        try
        {
            ES3.Save("relativeGamePaths", relativeGamePaths);
            UnityEngine.Debug.Log("relativeGamePaths���Z�[�u���܂����I");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("relativeGamePaths�̃Z�[�u�Ɏ��s���܂���: " + ex.Message);
        }
    }

    /// <summary>
    /// relativeGamePaths�����[�h����
    /// </summary>
    public void LoadRelativeGamePaths()
    {
        try
        {
            if (ES3.KeyExists("relativeGamePaths"))
            {
                relativeGamePaths = ES3.Load<List<string>>("relativeGamePaths");
                UnityEngine.Debug.Log("relativeGamePaths�����[�h���܂����I");
            }
            else
            {
                UnityEngine.Debug.Log("�ۑ����ꂽrelativeGamePaths��������܂���ł����B");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("relativeGamePaths�̃��[�h�Ɏ��s���܂���: " + ex.Message);
        }
    }

    private void Start()
    {
        // �Q�[���J�n���Ƀ��[�h
        LoadRelativeGamePaths();
    }

    private void OnApplicationQuit()
    {
        // �Q�[���I�����ɃZ�[�u
        SaveRelativeGamePaths();
    }
}
