﻿//#define LOG_FUNCTION_CALLS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)

public class HTKNetworkManager : NetworkManager {

    void Start()
    {
        /* Here you could configure Unity server settings:
		
        customConfig = true;
        connectionConfig.MaxCombinedReliableMessageCount = 40;
        connectionConfig.MaxCombinedReliableMessageSize = 800;
        connectionConfig.MaxSentMessageQueueSize = 2048;
        // https://forum.unity.com/threads/timeout-disconnect-after-a-few-minutes.367545/
        connectionConfig.NetworkDropThreshold = 90;
        // https://forum.unity.com/threads/matchmaker-client-timeout.342745/
        connectionConfig.PingTimeout = 5000;
        connectionConfig.DisconnectTimeout = 5000;
        connectionConfig.ConnectTimeout = 5000;

        //connectionConfig.NetworkDropThreshold = 45;
        connectionConfig.OverflowDropThreshold = 45;
       
        connectionConfig.AckDelay = 200;
        connectionConfig.AcksType = ConnectionAcksType.Acks96;
        connectionConfig.MaxSentMessageQueueSize = 256;

        globalConfig.ThreadAwakeTimeout = 1;
        */
    }

#if LOG_FUNCTION_CALLS
#if WINDOWS_UWP
    void LogEntry([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
    {
        Debug.Log(">> " + memberName);
    }
    void LogExit([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
    {
        Debug.Log("<< " + memberName);
    }
#else
    void LogEntry(string memberName = "")
    {
    Debug.Log("Need UWP");
    }

    void LogExit(string memberName = "")
    {
    Debug.Log("Need UWP");
    }
#endif
#else
    void LogEntry(string memberName = "")
    {
    }

    void LogExit(string memberName = "")
    {
    }
#endif

    public override void OnClientConnect(NetworkConnection conn)
    {
        LogEntry();
        base.OnClientConnect(conn);
        LogExit();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        LogEntry();
        base.OnClientError(conn, errorCode);
        LogExit();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        LogEntry();
        base.OnClientDisconnect(conn);
        LogExit();
    }

    public override void OnClientNotReady(NetworkConnection conn)
    {
        LogEntry();
        base.OnClientNotReady(conn);
        LogExit();
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        LogEntry();
        base.OnClientSceneChanged(conn);
        LogExit();

    }
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        LogEntry();
        base.OnDestroyMatch(success, extendedInfo);
        LogExit();
    }

    public override void OnDropConnection(bool success, string extendedInfo)
    {
        LogEntry();
        base.OnDropConnection(success, extendedInfo);
        LogExit();
    }

#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        LogEntry();
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        LogExit();
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        LogEntry();
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        LogExit();
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        LogEntry();
        base.OnMatchList(success, extendedInfo, matchList);
        LogExit();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        LogEntry();
        base.OnServerAddPlayer(conn, playerControllerId);
        LogExit();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        LogEntry();
        base.OnServerAddPlayer(conn, playerControllerId, extraMessageReader);
            
        if (conn.playerControllers.Count > 0)
        {
            GameObject player = conn.playerControllers[0].gameObject;
            var playersyncer = player.GetComponent<SharingSceneData>();
            // do stuff to the player GameObject
        }
        LogExit();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        LogEntry();
        base.OnServerConnect(conn);
        LogExit();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        LogEntry();
        base.OnServerDisconnect(conn);
        LogExit();
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        LogEntry();
        base.OnServerError(conn, errorCode);
        LogExit();
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        LogEntry();
        base.OnServerReady(conn);
        LogExit();
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        LogEntry();
        base.OnServerRemovePlayer(conn, player);
        LogExit();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        LogEntry();
        base.OnServerSceneChanged(sceneName);
        LogExit();
    }

    public override void OnSetMatchAttributes(bool success, string extendedInfo)
    {
        LogEntry();
        base.OnSetMatchAttributes(success, extendedInfo);
        LogExit();
    }

    public override void OnStartClient(NetworkClient client)
    {
        LogEntry();
        base.OnStartClient(client);
        LogExit();
    }

    public override void OnStartHost()
    {
        LogEntry();
        base.OnStartHost();
        LogExit();
    }

    public override void OnStartServer()
    {
        LogEntry();
        base.OnStartServer();
        LogExit();
    }

    public override void OnStopClient()
    {
        LogEntry();
        base.OnStopClient();
        LogExit();
    }

    public override void OnStopHost()
    {
        LogEntry();
        base.OnStopHost();
        LogExit();
    }

    public override void OnStopServer()
    {
        LogEntry();
        base.OnStopServer();
        LogExit();
    }

    public override void ServerChangeScene(string newSceneName)
    {
        LogEntry();
        base.ServerChangeScene(newSceneName);
        LogExit();
    }

    public override NetworkClient StartHost()
    {
        LogEntry();
        return base.StartHost();
    }

    public override NetworkClient StartHost(ConnectionConfig config, int maxConnections)
    {
        LogEntry();
        return base.StartHost(config, maxConnections);
    }

    public override NetworkClient StartHost(MatchInfo info)
    {
        LogEntry();
        return base.StartHost(info);
    }

    
}

#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
