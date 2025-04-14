using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogManager : MonoBehaviour
{
    private int num;
    public TextMeshProUGUI dialogText;
    public Animator animator;
    public AudioSource audioSource;

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
    }

    public DialogSet[] dialogTable = new DialogSet[10];
    [TextArea(2, 5)]
    public string[] fixedDialogTable = new string[10];

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
        int index = num;
        if (index >= 0 && index < dialogTable.Length)
        {
            var lines = dialogTable[index].lines;
            if (lines != null && lines.Count > 0)
            {
                string randomLine = lines[Random.Range(0, lines.Count)];
                SetDialog(randomLine);
            }
        }
    }

    public void SetFixedDialog(int index)
    {
        num = index;
        if (index >= 0 && index < fixedDialogTable.Length)
        {
            SetDialog(fixedDialogTable[index]);
        }
    }
}
