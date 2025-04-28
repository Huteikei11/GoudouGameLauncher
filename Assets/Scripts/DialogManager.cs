using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogManager : MonoBehaviour
{
    private int num;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI newsText;   // News 用の TextMeshProUGUI
    public TextMeshProUGUI detailText; // Detail 用の TextMeshProUGUI
    public Animator animator;
    public AudioSource audioSource;

    private int executionCount = 0; // 実行回数をカウントする変数
    private int previousNum = -1;  // 前回の num を記録する変数

    // 効果音クリップ名 → AudioClip の辞書（Inspectorで登録）
    [System.Serializable]
    public class SoundEntry
    {
        public string key;
        public AudioClip clip;
    }

    public List<SoundEntry> soundList = new List<SoundEntry>();
    private Dictionary<string, AudioClip> soundDict;

    [System.Serializable]
    public class DialogSet
    {
        [TextArea(2, 5)]
        public List<string> lines = new List<string>();
        [TextArea(2, 5)]
        public string news;   // 既存のフィールド
        [TextArea(2, 5)]
        public string detail; // 新しいフィールド
    }

    public DialogSet[] dialogTable = new DialogSet[10];


    private void Awake()
    {
        // AudioClip辞書を構築
        soundDict = new Dictionary<string, AudioClip>();
        foreach (var entry in soundList)
        {
            if (!soundDict.ContainsKey(entry.key))
            {
                soundDict.Add(entry.key, entry.clip);
            }
        }
    }

    // 正規表現パターン
    private static readonly Regex animRegex = new Regex(@"<anim:(.+?)>");
    private static readonly Regex waitRegex = new Regex(@"<wait:(\d+(\.\d+)?)>");
    private static readonly Regex soundRegex = new Regex(@"<sound:(.+?)>");

    /// <summary>
    /// セリフをコマンド付きで表示（コルーチン処理）
    /// </summary>
    public void SetDialog(string message)
    {
        StopAllCoroutines();
        StartCoroutine(HandleDialog(message));
    }

    private IEnumerator HandleDialog(string message)
    {
        // アニメーションコマンド
        var animMatch = animRegex.Match(message);
        if (animMatch.Success)
        {
            string trigger = animMatch.Groups[1].Value;
            animator?.SetTrigger(trigger);
            message = animRegex.Replace(message, "").Trim();
        }

        // 効果音コマンド
        var soundMatch = soundRegex.Match(message);
        if (soundMatch.Success)
        {
            string soundKey = soundMatch.Groups[1].Value;
            if (soundDict.TryGetValue(soundKey, out var clip))
            {
                audioSource?.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"効果音 '{soundKey}' が見つかりませんでした。");
            }
            message = soundRegex.Replace(message, "").Trim();
        }

        // 待機コマンド
        var waitMatch = waitRegex.Match(message);
        if (waitMatch.Success)
        {
            float waitTime = float.Parse(waitMatch.Groups[1].Value);
            message = waitRegex.Replace(message, "").Trim();
            yield return new WaitForSeconds(waitTime);
        }

        // 最終的なメッセージを表示
        dialogText.text = message;
    }

    public void SetDialogFromIndex()
    {
        // num が前回と異なる場合、実行回数をリセット
        if (num != previousNum)
        {
            executionCount = 0;
            previousNum = num; // 現在の num を記録
        }

        if (num >= 0 && num < dialogTable.Length)
        {
            var lines = dialogTable[num].lines;
            if (lines != null && lines.Count > 0)
            {
                // 実行回数をインデックスとして使用
                int lineIndex = executionCount % lines.Count; // 実行回数を lines.Count で割った余りを使用
                string line = lines[lineIndex];
                SetDialog(line);

                // 実行回数をインクリメント
                executionCount++;
            }
        }
        else
        {
            Debug.LogWarning("num の値が dialogTable の範囲外です。");
        }
    }

    public void SetFixedDialog(int index)//最初に表示される
    {
        num = index;
        if (index >= 0 && index < dialogTable.Length)
        {
            var lines = dialogTable[index].lines;
            if (lines != null && lines.Count > 0)
            {
                string randomLine = lines[0];
                SetDialog(randomLine);
            }
        }
        DisplayNewsAndDetail();
    }

    /// <summary>
    /// 指定したインデックスの news を取得
    /// </summary>
    public string GetNewsFromIndex(int index)
    {
        if (index >= 0 && index < dialogTable.Length)
        {
            return dialogTable[index].news;
        }
        return null;
    }

    /// <summary>
    /// 指定したインデックスの detail を取得
    /// </summary>
    public string GetDetailFromIndex(int index)
    {
        if (index >= 0 && index < dialogTable.Length)
        {
            return dialogTable[index].detail;
        }
        return null;
    }

    /// <summary>
    /// News と Detail を表示するメソッド
    /// </summary>
    public void DisplayNewsAndDetail()
    {
        if (num >= 0 && num < dialogTable.Length)
        {
            // News を表示
            if (newsText != null)
            {
                newsText.text = GetNewsFromIndex(num) ?? "No News Available";
            }

            // Detail を表示
            if (detailText != null)
            {
                detailText.text = GetDetailFromIndex(num) ?? "No Detail Available";
            }

            Debug.Log($"DisplayNewsAndDetail: num = {num}, news = {newsText.text}, detail = {detailText.text}"); // デバッグロ
        }
        else
        {
            Debug.LogWarning("num の値が dialogTable の範囲外です。");
        }
    }
}

