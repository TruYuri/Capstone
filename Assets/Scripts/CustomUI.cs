using System;
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

    public void ClickContinue()
    {
        GUIManager.Instance.ContinueAfterBattle(Convert.ToBoolean(data));
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
        GUIManager.Instance.SquadGroundAction(data);
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

    public void ClickSquadsList()
    {
        var split = data.Split('|');
        GUIManager.Instance.SetSquadList(Convert.ToBoolean(split[1]));

        if(split[1] == "true")
            data = split[0] + "|false";
        else
            data = split[0] + "|true";
    }

    public void ClickTileList()
    {
        var split = data.Split('|');
        GUIManager.Instance.SetTileList(Convert.ToBoolean(split[1]));

        if (split[1] == "true")
            data = split[0] + "|false";
        else
            data = split[0] + "|true";
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
