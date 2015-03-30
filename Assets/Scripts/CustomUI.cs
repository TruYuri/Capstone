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

    public void ClickManage()
    {
        GUIManager.Instance.PopulateManageLists();
    }

    public void CloseManage()
    {
        HumanPlayer.Instance.ReloadGameplayUI();
    }

    public void ClickDeploy()
    {
        if (data == "Deploy")
            HumanPlayer.Instance.Deploy(GUIManager.Instance.ListIndices["MainShipList"]);
        else
            HumanPlayer.Instance.Undeploy();
    }

    public void ClickListItem()
    {
        var split = data.Split('|');
        GUIManager.Instance.ListIndices[split[0]] = int.Parse(split[1]);
    }

    public void ClickConstructable()
    {

    }

	public void ResearchOpen()
	{
	}

	public void ResearchClose()
	{
	}

    public void TransferToControlledSquad()
    {
    }

    public void TransferAllToControlledSquad()
    {
    }

    public void TransferToSelectedSquad()
    {
    }

    public void TransferAllToSelectedSquad()
    {
    }

    public void Pause()
    {
        GameManager.Instance.Paused = true;
    }

    public void Unpause()
    {
        GameManager.Instance.Paused = false;
    }
}
