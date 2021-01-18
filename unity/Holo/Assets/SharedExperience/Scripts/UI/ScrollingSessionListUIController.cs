using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using HoloToolkit.Examples.SharingWithUNET;

public class ScrollingSessionListUIController : SingleInstance<ScrollingSessionListUIController>
{
    NetworkDiscoveryWithAnchors networkDiscovery;
    //CurrentSessionManager 
    Dictionary<string, NetworkDiscoveryWithAnchors.SessionInfo> sessionList;
    int SessionIndex = 0;

    public SessionListButton[] SessionControls;
    public NetworkDiscoveryWithAnchors.SessionInfo SelectedSession { get; private set; }

    // Use this for initialization
    void Start()
    {
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
        networkDiscovery.SessionListChanged += NetworkDiscovery_SessionListChanged;
        networkDiscovery.ConnectionStatusChanged += NetworkDiscovery_ConnectionStatusChanged;
        sessionList = networkDiscovery.remoteSessions;
        Debug.Log("Number of sessions: " + sessionList.Count().ToString());
        ScrollSessions(0);
    }

    private void NetworkDiscovery_ConnectionStatusChanged(object sender, EventArgs e)
    {
        SetChildren(networkDiscovery.running && !networkDiscovery.isServer);
    }

    private void NetworkDiscovery_SessionListChanged(object sender, EventArgs e)
    {
        sessionList = networkDiscovery.remoteSessions;
        // note that this looks off by one, but we're going to repurpose the last index to be the 
        // new session door, so it's okay. :)
        SessionIndex = Mathf.Min(SessionIndex, sessionList.Count);

        ScrollSessions(0);
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

    public void ScrollSessions(int Direction)
    {
        int sessionCount = sessionList == null ? 0 : sessionList.Count;
        SessionIndex = Mathf.Clamp(SessionIndex + Direction, 0, Mathf.Max(0,sessionCount - SessionControls.Length));
        for(int index=0;index<SessionControls.Length;index++)
        {
            if (SessionIndex + index < sessionCount)
            {
                SessionControls[index].gameObject.SetActive(true);
                NetworkDiscoveryWithAnchors.SessionInfo sessionInfo = sessionList.Values.ElementAt(SessionIndex + index);
                SessionControls[index].SetSessionInfo(sessionInfo);
            }
            else
            {
                SessionControls[index].gameObject.SetActive(false);
            }
        }
    }

    public void ClickSession(GameObject sessionButton)
    {
        const string SessionPrefix = "Session";
        int sessionInstanceIndex;
        if (int.TryParse(sessionButton.name.Substring(SessionPrefix.Length), out sessionInstanceIndex))
        {
            SetSelectedSession(sessionList.Values.ElementAt(sessionInstanceIndex));
        }
    }


    public void SetSelectedSession(NetworkDiscoveryWithAnchors.SessionInfo sessionInfo)
    {
        SelectedSession = sessionInfo;
        ScrollSessions(0);
    }

    public void JoinSelectedSession()
    {
        if (SelectedSession != null && networkDiscovery.running)
        {
            networkDiscovery.JoinSession(SelectedSession);
        }
    }
}
