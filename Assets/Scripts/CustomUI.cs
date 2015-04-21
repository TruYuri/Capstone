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
    }

    public void ClickBattle()
    {
        GUIManager.Instance.Battle();
    }

    public void ClickRetreat()
    {
        GUIManager.Instance.Retreat();
    }

    public void ClickWarp()
    {
        GUIManager.Instance.Warp();
    }

    public void SetScreen()
    {
        GUIManager.Instance.SetScreen(data);
    }

    public void ClickSwapManager()
    {
        GUIManager.Instance.SwapManager(data);
    }

    public void ClickDiplomacy()
    {
        HumanPlayer.Instance.CreateDiplomacyEvent();
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

    public void ClickSquadAction()
    {
        GUIManager.Instance.SquadAction(data);
    }

    public void ClickListItem()
    {
        GUIManager.Instance.ItemClicked(data);
    }

    public void ClickNewSquad()
    {
        GUIManager.Instance.NewSquad();
    }

    public void ClickShipTransfer()
    {
        GUIManager.Instance.ShipTransfer(data);
    }

    public void ClickSoldierTransfer()
    {
        GUIManager.Instance.SoldierTransfer(data);
    }

    public void ClickResourceTransfer()
    {
        GUIManager.Instance.ResourceTransfer(data);
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

    public void ClickTeam()
    {
        var gm = FindObjectOfType<GameManager>();
        gm.Init((Team)Enum.Parse(typeof(Team), data));
        this.transform.parent.gameObject.SetActive(false);
    }
}
