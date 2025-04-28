using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogGeter : MonoBehaviour
{
    public DialogSet[] dialogTable = new DialogSet[10];
    [SerializeField] private string accessKey;

    [System.Serializable]
    public class DialogSet
    {
        [TextArea(2, 5)]
        public List<string> lines = new List<string>();
        public string news;   // �����̃t�B�[���h
        public string detail; // �V�����t�B�[���h
    }

    public DialogManager dialogManager;

    private void Start()
    {
        StartCoroutine(UpdateDialogTableFromSpreadsheet());
    }

    private IEnumerator UpdateDialogTableFromSpreadsheet()
    {
        Debug.Log("�X�v���b�h�V�[�g����f�[�^���擾��...");
        var request = UnityWebRequest.Get("https://script.google.com/macros/s/" + accessKey + "/exec");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                Debug.Log("��M����JSON�f�[�^: " + request.downloadHandler.text);

                try
                {
                    // JSON�f�[�^���f�V���A���C�Y
                    var records = JsonUtility.FromJson<RawDialogRecords>(request.downloadHandler.text);

                    // dialogTable ���X�V
                    for (int i = 0; i < dialogTable.Length; i++)
                    {
                        if (i < records.records.Length)
                        {
                            var record = records.records[i];
                            var lines = new List<string>
                            {
                                record.lines0,
                                record.lines1,
                                record.lines2,
                                record.lines3,
                                record.lines4,
                                record.lines5,
                                record.lines6,
                                record.lines7,
                                record.lines8,
                                record.lines9,
                                record.lines10 // �V���� line10 ��ǉ�
                            };

                            dialogTable[i] = new DialogSet
                            {
                                lines = lines,
                                news = record.news,
                                detail = record.detail // �V�����t�B�[���h��ǉ�
                            };
                        }
                        else
                        {
                            dialogTable[i] = new DialogSet(); // ��̃Z�b�g���쐬
                        }
                    }

                    Debug.Log("dialogTable �̍X�V���������܂����I");

                    ApplyToDialogManager();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("�f�[�^����G���[: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("�f�[�^��M���s�F" + request.responseCode);
            }
        }
        else
        {
            Debug.LogError("�f�[�^��M���s: " + request.error);
        }
    }

    /// <summary>
    /// DialogManager �� dialogTable �� lines0 ���� line10 ��Ή�����C���f�b�N�X�ɒǉ����Anews �� detail �𔽉f
    /// </summary>
    public void ApplyToDialogManager()
    {
        if (dialogManager == null)
        {
            Debug.LogError("DialogManager ���w�肳��Ă��܂���I");
            return;
        }
        // DialogManager �� dialogTable �����Z�b�g
        for (int i = 0; i < dialogManager.dialogTable.Length; i++)
        {
            if (dialogManager.dialogTable[i] == null)
            {
                dialogManager.dialogTable[i] = new DialogManager.DialogSet();
            }
            else
            {
                dialogManager.dialogTable[i].lines.Clear(); // lines ���N���A
                dialogManager.dialogTable[i].news = null;   // news �����Z�b�g
                dialogManager.dialogTable[i].detail = null; // detail �����Z�b�g
            }
        }
        // dialogTable �̊e�v�f�� lines0 ���� line10 ��Ή����� dialogManager.dialogTable �ɒǉ�
        for (int i = 0; i < dialogTable.Length; i++)
        {
            if (dialogTable[i].lines != null)
            {
                for (int j = 0; j < dialogTable[i].lines.Count; j++) // lines0 ���� line10 �������ǉ�
                {
                    if (!string.IsNullOrEmpty(dialogTable[i].lines[j]) && j < dialogManager.dialogTable.Length)
                    {
                        dialogManager.dialogTable[j].lines.Add(dialogTable[i].lines[j]);
                    }
                }

                // news �� detail �𔽉f
                if (i < dialogManager.dialogTable.Length)
                {
                    dialogManager.dialogTable[i].news = dialogTable[i].news;
                    dialogManager.dialogTable[i].detail = dialogTable[i].detail;
                }
            }
        }

        Debug.Log("DialogManager �� dialogTable �� lines0 ���� line10 ��Ή�����C���f�b�N�X�ɒǉ����Anews �� detail �𔽉f���܂����I");
    }

    [System.Serializable]
    private class RawDialogRecord
    {
        public string lines0;
        public string lines1;
        public string lines2;
        public string lines3;
        public string lines4;
        public string lines5;
        public string lines6;
        public string lines7;
        public string lines8;
        public string lines9;
        public string lines10; // �V�����t�B�[���h
        public string news;   // �����̃t�B�[���h
        public string detail; // �V�����t�B�[���h
    }

    [System.Serializable]
    private class RawDialogRecords
    {
        public RawDialogRecord[] records;
    }
}
