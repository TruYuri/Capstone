using UnityEngine;
using System.Collections;

public class CustomUI : MonoBehaviour
{
    public string data;
    public bool disableAtStart;

    void Start()
    {
        GUIManager.Instance.Register(data, this, disableAtStart);
    }

    public void ClickUpgrade()
    {
        var split = data.Split('|');
        HumanPlayer.Instance.UpgradeResearch(split[0], split[1], split[2]);
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
        GUIManager.Instance.SetStructure(data);
    }

    public void ClickListItem()
    {
        GUIManager.Instance.ItemClicked(data);
    }

    public void ClickNewSquad()
    {
        GUIManager.Instance.NewSquad();
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

    public void ClickSoldierTransfer()
    {
        GUIManager.Instance.SoldierTransfer(data);
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
