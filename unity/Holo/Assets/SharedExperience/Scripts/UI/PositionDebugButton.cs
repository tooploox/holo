using HoloToolkit.Examples.SharingWithUNET;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PositionDebugButton : ScriptableSingleton<PositionDebugButton> {

    public GameObject DisconnectedPosition { get; set; }
    public GameObject ConnectedPosition { get; set; }

    NetworkDiscoveryWithAnchors networkDisco;

	// Use this for initialization
	void Start ()
    {
	    // Do not change parent, keep debug button inside side menu, this way it is also not hidden after "Start"
        transform.SetParent(DisconnectedPosition.transform, true);
        networkDisco = NetworkDiscoveryWithAnchors.Instance;
        networkDisco.ConnectionStatusChanged += NetworkDisco_ConnectionStatusChanged;
	}

    private void NetworkDisco_ConnectionStatusChanged(object sender, System.EventArgs e)
    {
        MoveToRightSpot();
    }

    void MoveToRightSpot()
    {
        GameObject parent = networkDisco.Connected ? ConnectedPosition : DisconnectedPosition;
        if (parent == null)
        {
            Invoke("MoveToRightSpot", 0.1f);
            return;
        }

        transform.SetParent(parent.transform, true);
        // this is a little hack because our parent might have disabled our renderers/colliders. 
        SetChildren(true);
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
}
