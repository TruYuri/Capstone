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
        GUIManager.Instance.Battle();
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
        GUIManager.Instance.ExitManage();
    }

    public void ClickDeploy()
    {
        GUIManager.Instance.SetStructure(data == "Deploy" ? false : true);
    }

    public void ClickListItem()
    {
        GUIManager.Instance.ItemClicked(data);
    }

    public void ClickNewSquad()
    {
        GUIManager.Instance.NewSquad();
    }

    public void ClickConstructable()
    {

    }

    public void TransferToControlledSquad()
    {
        GUIManager.Instance.TransferToControlledSquad();
    }

    public void TransferAllToControlledSquad()
    {
        GUIManager.Instance.TransferAllToControlledSquad();
    }

    public void TransferToSelectedSquad()
    {
        GUIManager.Instance.TransferToSelectedSquad();
    }

    public void TransferAllToSelectedSquad()
    {
        GUIManager.Instance.TransferAllToSelectedSquad();
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
