using System;
using UnityEngine;
using System.Collections;

public class CustomUI : MonoBehaviour
{
    public string data;
    public bool disableAtStart;
    public bool dontRegister;

    void Start()
    {
    }
    
    public void Register()
    {
        if (!dontRegister)
            GUIManager.Instance.Register(data, this, disableAtStart);
    }

    public void ClickUpgrade()
    {
        GUIManager.Instance.UpdateResearch(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickBattle()
    {
        GUIManager.Instance.Battle();
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickRetreat()
    {
        GUIManager.Instance.Retreat();
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickWarp()
    {
        GUIManager.Instance.Warp();
    }

    public void SetScreen()
    {
        GUIManager.Instance.SetScreen(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickSwapManager()
    {
        GUIManager.Instance.SwapManager(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickDiplomacy()
    {
        HumanPlayer.Instance.CreateDiplomacyEvent();
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickContinue()
    {
        GUIManager.Instance.ContinueAfterBattle(Convert.ToBoolean(data));
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickManage()
    {
        GUIManager.Instance.PopulateManageLists();
        GUIManager.Instance.PlaySound("Click");
    }

    public void CloseManage()
    {
        GUIManager.Instance.ExitManage();
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickSquadAction()
    {
        GUIManager.Instance.SquadAction(data);
        GUIManager.Instance.PlaySound("Command");
    }

    public void ClickListItem()
    {
        GUIManager.Instance.ItemClicked(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickNewSquad()
    {
        GUIManager.Instance.NewSquad();
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickShipTransfer()
    {
        GUIManager.Instance.ShipTransfer(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickSoldierTransfer()
    {
        GUIManager.Instance.SoldierTransfer(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickResourceTransfer()
    {
        GUIManager.Instance.ResourceTransfer(data);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickSquadsList()
    {
        var split = data.Split('|');
        GUIManager.Instance.SetSquadList(Convert.ToBoolean(split[1]));

        if(split[1] == "true")
            data = split[0] + "|false";
        else
            data = split[0] + "|true";
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickTileList()
    {
        var split = data.Split('|');
        GUIManager.Instance.SetTileList(Convert.ToBoolean(split[1]));

        if (split[1] == "true")
            data = split[0] + "|false";
        else
            data = split[0] + "|true";
        GUIManager.Instance.PlaySound("Click");
    }

    public void Pause()
    {
        GameManager.Instance.Paused = true;
    }

    public void Unpause()
    {
        GameManager.Instance.Paused = false;
    }

    public void ClickTeam()
    {
        var gm = FindObjectOfType<GameManager>();
        gm.Init((Team)Enum.Parse(typeof(Team), data));
        this.transform.parent.gameObject.SetActive(false);
        GUIManager.Instance.PlaySound("Click");
    }

    public void ClickZoom()
    {
        GUIManager.Instance.SetZoom(data, true);
        GUIManager.Instance.PlaySound("Click");
    }
}
