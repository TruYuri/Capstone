using UnityEngine;
using System.Collections;

public class CustomUI : MonoBehaviour 
{
    public string data;

    void Start()
    {
        GUIManager.Instance.Register(data, this);
    }

    public void ClickUpgrade()
    {
        var split = data.Split('|');
        var upgrade = Player.Instance.UpgradeResearch(split[0], split[1], split[2]);
    }

    public void ClickBattle()
    {

    }

    public void ClickRetreat()
    {

    }

    public void ClickMerge()
    {

    }

    public void ClickDeploy()
    {
        Player.Instance.Deploy(GUIManager.Instance.ListIndex);
    }

    public void ClickSplit()
    {

    }

    public void ClickListItem()
    {
        GUIManager.Instance.ListIndex = int.Parse(data);
    }
}
