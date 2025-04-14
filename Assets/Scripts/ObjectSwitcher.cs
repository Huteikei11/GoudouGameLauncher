using System.Collections.Generic;
using UnityEngine;

public class ObjectSwitcher : MonoBehaviour
{
    // Inspectorから登録する任意のGameObjectリスト
    public List<GameObject> objectList;

    // 指定されたインデックスのオブジェクトだけ表示（他は非表示）
    public void SwitchObject(int index)
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            if (objectList[i] != null)
            {
                objectList[i].SetActive(i == index);
            }
        }
    }
}
