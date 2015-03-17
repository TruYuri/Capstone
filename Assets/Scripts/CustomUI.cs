using UnityEngine;
using System.Collections;

public class CustomUI : MonoBehaviour 
{
    public string data;

    void Start()
    {
        // GUIManager.Instance.Register(data, this);
    }

    public void ClickUpgrade()
    {
        var split = data.Split('|');
        var upgrade = Player.Instance.UpgradeResearch(split[0], split[1], split[2]);
    }
}
