using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CustomUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string data;

    void Start()
    {
        GUIManager.Instance.Register(data, this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    { 
    }

    public void OnPointerExit(PointerEventData eventData)
    { 
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

    public void Pause()
    {
        GameManager.Instance.Paused = true;
    }

    public void Unpause()
    {
        GameManager.Instance.Paused = false;
    }
}
