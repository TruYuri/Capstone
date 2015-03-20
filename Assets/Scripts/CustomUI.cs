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
        var upgrade = HumanPlayer.Instance.UpgradeResearch(split[0], split[1], split[2]);
    }

    public void ClickBattle()
    {
        HumanPlayer.Instance.Battle();
    }

    public void ClickRetreat()
    {

    }

    public void ClickMerge()
    {

    }

    public void ClickDeploy()
    {
        HumanPlayer.Instance.Deploy(GUIManager.Instance.ListIndex);
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
