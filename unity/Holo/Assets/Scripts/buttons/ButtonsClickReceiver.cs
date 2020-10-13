using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

/* Add this script as a component of GameObject, sibling of any other script
 * that implements IClickHandler interface.
 * It will detect clicks on buttons (on Interactables list, inherited from InteractionReceiver),
 * and call IClickHandler.Click when necessary.
 * 
 * There may be multiple siblings implementing IClickHandler, all will be notified
 * about events. This allows to split code across multiple scripts.
 */
public class ButtonsClickReceiver : IMixedRealityInputHandler, IMixedRealityFocusHandler
{
	void Start()
	{
	}
    
    // Track clicking interactable things. 
    // You need to make InputDown, then InputUp on the same GameObject to register as click.
    private GameObject clicking;

    private void ShowToolTip(GameObject obj, bool show)
    {
        Transform tooltip = obj.transform.GetChild(obj.transform.childCount - 1);
        if (tooltip.name == "ToolTip") tooltip.gameObject.SetActive(show);
    }

    public void OnInputUp(InputEventData eventData)
    {
        //Debug.Log(obj.name + " : InputUp");
        if (clicking == eventData.selectedObject)
        {
            foreach (IClickHandler clickHandler in eventData.selectedObject.GetComponents<IClickHandler>())
            {
                clickHandler.Click(eventData.selectedObject);
            }
        }
        clicking = null;
    }

    public void OnInputDown(InputEventData eventData)
    {
        //Debug.Log(obj.name + " : InputDown");
        clicking = eventData.selectedObject;
        ShowToolTip(eventData.selectedObject, false);
    }

    public void OnFocusEnter(FocusEventData eventData)
    {
        //Debug.Log(obj.name + " : FocusEnter");
        ShowToolTip(eventData.selectedObject, true);
        foreach (IClickHandler clickHandler in eventData.selectedObject.GetComponents<IClickHandler>())
        {
            clickHandler.FocusEnter(eventData.selectedObject);
        }
    }

    public void OnFocusExit(FocusEventData eventData)
    {
        //Debug.Log(obj.name + " : FocusExit");
        ShowToolTip(eventData.selectedObject, false);
        foreach (IClickHandler clickHandler in eventData.selectedObject.GetComponents<IClickHandler>())
        {
            clickHandler.FocusExit(eventData.selectedObject);
        }
    }
}
