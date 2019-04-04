using UnityEngine;
using HoloToolkit.Unity.Receivers;
using HoloToolkit.Unity.InputModule;

/* Add this script as a component of GameObject, sibling of any other script
 * that implements IClickHandler interface.
 * It will detect clicks on buttons (on Interactables list, inherited from InteractionReceiver),
 * and call IClickHandler.Click when necessary.
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
	}

	protected override void FocusExit(GameObject obj, PointerSpecificEventData eventData)
    {
        //Debug.Log(obj.name + " : FocusExit");
    }

	protected override void InputDown(GameObject obj, InputEventData eventData)
    {
		//Debug.Log(obj.name + " : InputDown");
        clicking = obj;
    }

	protected override void InputUp(GameObject obj, InputEventData eventData)
    {
		//Debug.Log(obj.name + " : InputUp");
        if (clicking == obj)
        {
            IClickHandler clickHandler = GetComponent<IClickHandler>();
            clickHandler.Click(obj);
        }
        clicking = null;
	}
}
