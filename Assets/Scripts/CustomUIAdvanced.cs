using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Enhanced version of CustomUI that enables highlighting and hover capabilities.
/// </summary>
public class CustomUIAdvanced : CustomUI, IPointerEnterHandler, IPointerExitHandler 
{
    private bool highlighted; // is this object currently highlighted?

    /// <summary>
    /// Updates the UI when the player highlights this object.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        GUIManager.Instance.UIHighlighted(data);
        highlighted = true;
    }

    /// <summary>
    /// Updates the UI when the player dehighlights this object.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        GUIManager.Instance.UIDehighlighted(data);
        highlighted = false;
    }

    /// <summary>
    /// Updates the UI if the player continues to highlight this object.
    /// </summary>
    void Update()
    {
        if (highlighted)
            GUIManager.Instance.UIHighlightStay(data);
    }
}
