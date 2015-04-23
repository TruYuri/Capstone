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
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickBattle()
    {
        GUIManager.Instance.Battle();
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickRetreat()
    {
        GUIManager.Instance.Retreat();
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickWarp()
    {
        GUIManager.Instance.Warp();
        GUIManager.Instance.PlayButtonClick();
    }

    public void SetScreen()
    {
        GUIManager.Instance.SetScreen(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickSwapManager()
    {
        GUIManager.Instance.SwapManager(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickDiplomacy()
    {
        HumanPlayer.Instance.CreateDiplomacyEvent();
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickContinue()
    {
        GUIManager.Instance.ContinueAfterBattle(Convert.ToBoolean(data));
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickManage()
    {
        GUIManager.Instance.PopulateManageLists();
        GUIManager.Instance.PlayButtonClick();
    }

    public void CloseManage()
    {
        GUIManager.Instance.ExitManage();
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickSquadAction()
    {
        GUIManager.Instance.SquadAction(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickListItem()
    {
        GUIManager.Instance.ItemClicked(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickNewSquad()
    {
        GUIManager.Instance.NewSquad();
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickShipTransfer()
    {
        GUIManager.Instance.ShipTransfer(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickSoldierTransfer()
    {
        GUIManager.Instance.SoldierTransfer(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickResourceTransfer()
    {
        GUIManager.Instance.ResourceTransfer(data);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickSquadsList()
    {
        GUIManager.Instance.PlayButtonClick();
        var split = data.Split('|');
        GUIManager.Instance.SetSquadList(Convert.ToBoolean(split[1]));

        if(split[1] == "true")
            data = split[0] + "|false";
        else
            data = split[0] + "|true";
    }

    public void ClickTileList()
    {
        GUIManager.Instance.PlayButtonClick();
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

    public void ClickTeam()
    {
        var gm = FindObjectOfType<GameManager>();
        gm.Init((Team)Enum.Parse(typeof(Team), data));
        this.transform.parent.gameObject.SetActive(false);
        GUIManager.Instance.PlayButtonClick();
    }

    public void ClickZoom()
    {
        GUIManager.Instance.SetZoom(data, true);
        GUIManager.Instance.PlayButtonClick();
    }
}
