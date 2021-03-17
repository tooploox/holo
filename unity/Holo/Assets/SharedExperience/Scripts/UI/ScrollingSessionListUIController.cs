using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using HoloToolkit.Examples.SharingWithUNET;

public class ScrollingSessionListUIController : SingleInstance<ScrollingSessionListUIController>
{
    NetworkDiscoveryWithAnchors networkDiscovery;
    //CurrentSessionManager 
    Dictionary<string, NetworkDiscoveryWithAnchors.SessionInfo> sessionList;

    public GameObject SessionSearch;
    public GameObject GridObjectCollection;

    public NetworkDiscoveryWithAnchors.SessionInfo SelectedSession { get; private set; }

    // Use this for initialization
    void Start()
    {
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
        networkDiscovery.SessionListChanged += NetworkDiscovery_SessionListChanged;
        networkDiscovery.ConnectionStatusChanged += NetworkDiscovery_ConnectionStatusChanged;
        sessionList = networkDiscovery.remoteSessions;
        Debug.Log("Number of sessions: " + sessionList.Count().ToString());
    }

    private void NetworkDiscovery_ConnectionStatusChanged(object sender, EventArgs e)
    {
        SetChildren(networkDiscovery.running && !networkDiscovery.isServer);
    }

    private void NetworkDiscovery_SessionListChanged(object sender, EventArgs e)
    {
        bool sessionsFound = sessionList.Count > 0;
        SessionSearch.SetActive(!sessionsFound);
        GridObjectCollection.SetActive(sessionsFound);

        List<GameObject> buttonList = GridObjectCollection.GetComponent<ButtonListScript>().ButtonList;
        int buttonNumber = 0;
        foreach (KeyValuePair<string, NetworkDiscoveryWithAnchors.SessionInfo> sessionEntry in networkDiscovery.remoteSessions)
        {
            GameObject CurrentSessionButton = buttonList[buttonNumber];
            CurrentSessionButton.GetComponent<SessionListButton>().SetSessionInfo(sessionEntry.Value);
            if (buttonNumber == 9) break;
            buttonNumber++;
        }

        if (buttonNumber < 9)
        {
            for (int i = buttonNumber; i <= 9; i++)
            {
                GameObject CurrentSessionButton = GridObjectCollection.transform.Find("SessionButton" + i.ToString()).gameObject;
                CurrentSessionButton.GetComponent<SessionListButton>().SetSessionInfo(null);
            }
        }

    }

    void SetChildren(bool Enabled)
    {
        foreach (Renderer mr in GetComponentsInChildren<Renderer>())
        {
            mr.enabled = Enabled;
        }

        foreach (BoxCollider bc in GetComponentsInChildren<BoxCollider>())
        {
            bc.enabled = Enabled;
        }
    }

    public void SetSelectedSession(NetworkDiscoveryWithAnchors.SessionInfo sessionInfo)
    {
        SelectedSession = sessionInfo;
    }

    public bool JoinSelectedSession()
    {
        if (SelectedSession != null && networkDiscovery.running)
        {
            networkDiscovery.JoinSession(SelectedSession);
            return true;
        }
        else
        {
            return false;
        }
    }
}
