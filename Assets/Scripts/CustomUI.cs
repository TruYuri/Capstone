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
        Player.Instance.Battle();
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
        // show split list screen
    }

    public void ClickListItem()
    {
        GUIManager.Instance.ListIndex = int.Parse(data);
    }

	public void ResearchOpen()
	{
		GameManager.Instance.Paused = true;
	}

	public void ResearchClose()
	{
		GameManager.Instance.Paused = false;
	}
}
