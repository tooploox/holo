using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonListScript : MonoBehaviour
{
    public List<GameObject> ButtonList = new List<GameObject>();

    public void DeselectAllButtons()
    {
        foreach (GameObject button in ButtonList)
        {
            button.GetComponent<SessionListButton>().DeselectSession();
        }
    }
}
