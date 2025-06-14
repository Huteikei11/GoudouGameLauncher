using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButton : MonoBehaviour
{
    public int gameNumber;
    [SerializeField] private DialogManager dialogManager;
    [SerializeField] private GameLauncherSystem GameLauncherSystem;
    // Start is called before the first frame update
    void Start()
    {
        Select(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(int num)
    {
        gameNumber = num;
        dialogManager.SetFixedDialog(num);
        GameLauncherSystem.SetLaunchIndex(num);
    }
}
