using System;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

/// <summary>
/// Custom script attached to UI elements to expand their functionality.
/// </summary>
public class CustomUI : MonoBehaviour
{
    public string data; // data specific to the UI object
    public bool disableAtStart; // should this be disabled at game boot?
    public bool dontRegister; // don't registered this with the GUIManager.
    
    /// <summary>
    /// Registers this script with the GUIManager.
    /// </summary>
    public void Register()
    {
        if (!dontRegister)
            GUIManager.Instance.Register(data, this, disableAtStart);
    }

    /// <summary>
    /// Used when a research upgrade is clicked.
    /// </summary>
    public void ClickUpgrade()
    {
        GUIManager.Instance.UpdateResearch(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when "Battle" is selected.
    /// </summary>
    public void ClickBattle()
    {
        GUIManager.Instance.Battle();
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when "Retreat" is selected.
    /// </summary>
    public void ClickRetreat()
    {
        GUIManager.Instance.Retreat();
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when "Warp" is selected.
    /// </summary>
    public void ClickWarp()
    {
        GUIManager.Instance.Warp();
    }

    /// <summary>
    /// Used when the screen must change, i.e. research transitioning to gameplay.
    /// </summary>
    public void SetScreen()
    {
        GUIManager.Instance.SetScreen(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player swaps the management type in the ship management interface.
    /// </summary>
    public void ClickSwapManager()
    {
        GUIManager.Instance.SwapManager(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player selects "Diplomacy"
    /// </summary>
    public void ClickDiplomacy()
    {
        HumanPlayer.Instance.CreateDiplomacyEvent();
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player is finished viewing battle results.
    /// </summary>
    public void ClickContinue()
    {
        GUIManager.Instance.ContinueAfterBattle(Convert.ToBoolean(data));
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Opens the management interface.
    /// </summary>
    public void ClickManage()
    {
        GUIManager.Instance.PopulateManageLists();
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Closes the management interface.
    /// </summary>
    public void CloseManage()
    {
        GUIManager.Instance.ExitManage();
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used for Invade, Warp, Deploy, and Undeploy.
    /// </summary>
    public void ClickSquadAction()
    {
        GUIManager.Instance.SquadAction(data);
        GUIManager.Instance.PlaySound("Command");
    }

    /// <summary>
    /// Used when the player chooses an item in ANY list.
    /// </summary>
    public void ClickListItem()
    {
        GUIManager.Instance.ItemClicked(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player chooses to create a new squad.
    /// </summary>
    public void ClickNewSquad()
    {
        GUIManager.Instance.NewSquad();
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player clicks to transfer a ship.
    /// </summary>
    public void ClickShipTransfer()
    {
        GUIManager.Instance.ShipTransfer(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player clicks to transfer a soldier.
    /// </summary>
    public void ClickSoldierTransfer()
    {
        GUIManager.Instance.SoldierTransfer(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player clicks to transfer a resource.
    /// </summary>
    public void ClickResourceTransfer()
    {
        GUIManager.Instance.ResourceTransfer(data);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Used when the player opens or closes the squads list.
    /// </summary>
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

    /// <summary>
    /// Used when the player opens or closes the Tile list.
    /// </summary>
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

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void Pause()
    {
        GameManager.Instance.Paused = true;
    }

    /// <summary>
    /// Unpauses the game.
    /// </summary>
    public void Unpause()
    {
        GameManager.Instance.Paused = false;
    }

    /// <summary>
    /// Selects a team at game start.
    /// </summary>
    public void ClickTeam()
    {
        var gm = FindObjectOfType<GameManager>();
        gm.Init((Team)Enum.Parse(typeof(Team), data));
        this.transform.parent.gameObject.SetActive(false);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Zooms or unzooms the minimap.
    /// </summary>
    public void ClickZoom()
    {
        GUIManager.Instance.SetZoom(data, true);
        GUIManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Enables or disables the background music.
    /// </summary>
    public void ClickMusic()
    {
        if(data == "on")
        {
            this.transform.FindChild("Text").GetComponent<Text>().text = "Music - off";
            data = "off";
            HumanPlayer.Instance.GetComponent<AudioSource>().Pause();
        }
        else
        {
            this.transform.FindChild("Text").GetComponent<Text>().text = "Music - on";
            data = "on";
            HumanPlayer.Instance.GetComponent<AudioSource>().UnPause();
        }
    }

    /// <summary>
    /// Exits the game.
    /// </summary>
    public void ClickExit()
    {
        Application.Quit();
    }
}
