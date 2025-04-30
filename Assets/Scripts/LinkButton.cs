using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    public string URL;

    public void SetURL(string seturl)
    {
        URL = seturl;
    }
    public void onClick()
    {
        Application.OpenURL(URL);//""の中には開きたいWebページのURLを入力します
    }
}