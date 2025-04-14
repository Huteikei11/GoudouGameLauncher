using System.Collections.Generic;
using UnityEngine;

public class ObjectSwitcher : MonoBehaviour
{
    // Inspector����o�^����C�ӂ�GameObject���X�g
    public List<GameObject> objectList;

    // �w�肳�ꂽ�C���f�b�N�X�̃I�u�W�F�N�g�����\���i���͔�\���j
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
