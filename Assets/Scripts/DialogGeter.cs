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
        public string news;   // 既存のフィールド
        public string detail; // 新しいフィールド
        public string URL0;   // 新しいフィールド
        public string URL1;   // 新しいフィールド
        public string URL2;   // 新しいフィールド
    }

    public DialogManager dialogManager;

    private void Start()
    {
        StartCoroutine(UpdateDialogTableFromSpreadsheet());
    }

    private IEnumerator UpdateDialogTableFromSpreadsheet()
    {
        Debug.Log("スプレッドシートからデータを取得中...");
        var request = UnityWebRequest.Get("https://script.google.com/macros/s/" + accessKey + "/exec");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 200)
            {
                Debug.Log("受信したJSONデータ: " + request.downloadHandler.text);

                try
                {
                    // JSONデータをデシリアライズ
                    var records = JsonUtility.FromJson<RawDialogRecords>(request.downloadHandler.text);

                    // dialogTable を更新
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
                                record.lines10 // 新しい line10 を追加
                            };

                            dialogTable[i] = new DialogSet
                            {
                                lines = lines,
                                news = record.news,
                                detail = record.detail,
                                URL0 = record.URL0, 
                                URL1 = record.URL1, 
                                URL2 = record.URL2
                            };

                            Debug.Log($"Index {i}: news = {record.news}, detail = {record.detail}"); // デバッグログを追加

                        }
                        else
                        {
                            dialogTable[i] = new DialogSet(); // 空のセットを作成
                        }
                    }

                    Debug.Log("dialogTable の更新が完了しました！");

                    ApplyToDialogManager();
                    dialogManager.SetFixedDialog(0);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("データ代入エラー: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("データ受信失敗：" + request.responseCode);
            }
        }
        else
        {
            Debug.LogError("データ受信失敗: " + request.error);
        }
    }

    /// <summary>
    /// DialogManager の dialogTable に lines0 から line10 を対応するインデックスに追加し、news と detail を反映
    /// </summary>
    public void ApplyToDialogManager()
    {
        if (dialogManager == null)
        {
            Debug.LogError("DialogManager が指定されていません！");
            return;
        }
        // DialogManager の dialogTable をリセット
        for (int i = 0; i < dialogManager.dialogTable.Length; i++)
        {
            if (dialogManager.dialogTable[i] == null)
            {
                dialogManager.dialogTable[i] = new DialogManager.DialogSet();
            }
            else
            {
                dialogManager.dialogTable[i].lines.Clear(); // lines をクリア
                dialogManager.dialogTable[i].news = null;   // news をリセット
                dialogManager.dialogTable[i].detail = null; // detail をリセット
                dialogManager.dialogTable[i].URL0 = null; 
                dialogManager.dialogTable[i].URL1 = null; 
                dialogManager.dialogTable[i].URL2 = null;
            }
        }
        // dialogTable の各要素の lines0 から line10 を対応する dialogManager.dialogTable に追加
        for (int i = 0; i < dialogTable.Length; i++)
        {
            if (dialogTable[i].lines != null)
            {
                for (int j = 0; j < dialogTable[i].lines.Count; j++) // lines0 から line10 を順次追加
                {
                    if (!string.IsNullOrEmpty(dialogTable[i].lines[j]) && j < dialogManager.dialogTable.Length)
                    {
                        dialogManager.dialogTable[j].lines.Add(dialogTable[i].lines[j]);
                    }
                }

                // news と detail を反映
                if (i < dialogManager.dialogTable.Length)
                {
                    dialogManager.dialogTable[i].news = dialogTable[i].news;
                    dialogManager.dialogTable[i].detail = dialogTable[i].detail;
                    dialogManager.dialogTable[i].URL0 = dialogTable[i].URL0;
                    dialogManager.dialogTable[i].URL1 = dialogTable[i].URL1;
                    dialogManager.dialogTable[i].URL2 = dialogTable[i].URL2;

                    Debug.Log($"DialogManager Index {i}: news = {dialogManager.dialogTable[i].news}, detail = {dialogManager.dialogTable[i].detail}"); // デバッグログを追加

                }
            }
        }

        Debug.Log("DialogManager の dialogTable に lines0 から line10 を対応するインデックスに追加し、news と detail を反映しました！");
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
        public string lines10; // 新しいフィールド
        public string news;   // 既存のフィールド
        public string detail; // 新しいフィールド
        public string URL0;    // 新しいフィールド
        public string URL1;    // 新しいフィールド
        public string URL2;
    }

    [System.Serializable]
    private class RawDialogRecords
    {
        public RawDialogRecord[] records;
    }
}
