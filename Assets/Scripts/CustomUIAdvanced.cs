using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CustomUIAdvanced : CustomUI, IPointerEnterHandler, IPointerExitHandler 
{
    private bool highlighted;
	// Use this for initialization
	void Start () 
    {
        GUIManager.Instance.Register(data, this, disableAtStart);
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        GUIManager.Instance.UIHighlighted(data);
        highlighted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GUIManager.Instance.UIDehighlighted(data);
        highlighted = false;
    }

    void Update()
    {
        if (highlighted)
            GUIManager.Instance.UIHighlightStay(data);
    }
}
