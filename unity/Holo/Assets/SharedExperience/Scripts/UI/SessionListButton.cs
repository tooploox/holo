using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using HoloToolkit.Examples.SharingWithUNET;
public class SessionListButton : MonoBehaviour
{


    int textColorId;
    public GameObject TextGameObject;

    private ScrollingSessionListUIController scrollingUIController;
    private NetworkDiscoveryWithAnchors.SessionInfo SessionInfo;
  
    void OnEnable()
    {
        scrollingUIController = ScrollingSessionListUIController.Instance;
    }

    private bool isSelected = false;
    public void Click()
    {
        if (!isSelected)
        {
            SelectSession();
        }
        else
        {
            DeselectSession();
        }
        
    }

    public void SetSessionInfo(NetworkDiscoveryWithAnchors.SessionInfo sessionInfo )
    {
        if (sessionInfo != null)
        {
            gameObject.SetActive(true);
            SessionInfo = sessionInfo;
            //FIXME: String Concatenation doesnt work for some reason
            string buttonString = String.Format("{0}: {1}", SessionInfo.SessionName, SessionInfo.SessionIp);
            TextGameObject.GetComponent<TextMeshPro>().text = buttonString;
        }
        else
        {
            SessionInfo = null;
            gameObject.SetActive(false);
        }
    }

    public void SelectSession()
    {

        gameObject.GetComponentInParent<ButtonListScript>().DeselectAllButtons();
        TextGameObject.GetComponent<TextMeshPro>().color = new Color(0f, 0.90f, 0.88f);
        scrollingUIController.SetSelectedSession(SessionInfo);
        isSelected = true;
    }

    public void DeselectSession()
    {
        if (gameObject.activeSelf)
        {
            TextGameObject.GetComponent<TextMeshPro>().color = Color.white;
            scrollingUIController.SetSelectedSession(null);
            isSelected = false;
        }
    }
}
