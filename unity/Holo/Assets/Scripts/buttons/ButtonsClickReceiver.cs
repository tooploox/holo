using UnityEngine;
using HoloToolkit.Unity.Receivers;
using HoloToolkit.Unity.InputModule;

/* Add this script as a component of GameObject, sibling of any other script
 * that implements IClickHandler interface.
 * It will detect clicks on buttons (on Interactables list, inherited from InteractionReceiver),
 * and call IClickHandler.Click when necessary.
 * 
 * There may be multiple siblings implementing IClickHandler, all will be notified
 * about events. This allows to split code across multiple scripts.
 */
public class ButtonsClickReceiver : InteractionReceiver
{
	void Start()
	{
	}
    
    // Track clicking interactable things. 
    // You need to make InputDown, then InputUp on the same GameObject to register as click.
    private GameObject clicking;

    protected override void FocusEnter(GameObject obj, PointerSpecificEventData eventData)
    {
        //Debug.Log(obj.name + " : FocusEnter");
        ShowToolTip(obj, true);
        foreach (IClickHandler clickHandler in GetComponents<IClickHandler>()) {
            clickHandler.FocusEnter(obj);
        }
    }

    protected override void FocusExit(GameObject obj, PointerSpecificEventData eventData)
    {
        //Debug.Log(obj.name + " : FocusExit");
        ShowToolTip(obj, false);
        foreach (IClickHandler clickHandler in GetComponents<IClickHandler>()) { 
            clickHandler.FocusExit(obj);
        }
    }
    
	protected override void InputDown(GameObject obj, InputEventData eventData)
    {
		//Debug.Log(obj.name + " : InputDown");
        clicking = obj;
        ShowToolTip(obj, false);
    }

	protected override void InputUp(GameObject obj, InputEventData eventData)
    {
		//Debug.Log(obj.name + " : InputUp");
        if (clicking == obj) {
            foreach (IClickHandler clickHandler in GetComponents<IClickHandler>()) {
                clickHandler.Click(obj);
            }
        }
        clicking = null;
	}

    private void ShowToolTip(GameObject obj, bool show)
    {
        Transform tooltip = obj.transform.GetChild(obj.transform.childCount - 1);
        if (tooltip.name == "ToolTip") tooltip.gameObject.SetActive(show);
    }
}
