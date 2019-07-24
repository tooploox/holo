// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine.XR.WSA.Input;

#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)

namespace HoloToolkit.Examples.SharingWithUNET
{
    /// <summary>
    /// Controls player behavior (local and remote).
    /// </summary>
    [NetworkSettings(sendInterval = 0.033f)]
    public class PlayerController : NetworkBehaviour
    {
        private static PlayerController _Instance = null;
        public static PlayerController Instance
        {
            get
            {
                return _Instance;
            }
        }

        public static List<PlayerController> allPlayers = new List<PlayerController>();

        /// <summary>
        /// The transform of the shared world anchor.
        /// </summary>
        private Transform sharedWorldAnchorTransform;

        private UNetAnchorManager anchorManager;

        /// <summary>
        /// The position relative to the shared world anchor.
        /// </summary>
        [SyncVar]
        private Vector3 localPosition;

        /// <summary>
        /// The rotation relative to the shared world anchor.
        /// </summary>
        [SyncVar]
        private Quaternion localRotation;

        /// <summary>
        /// Sets the localPosition and localRotation on clients.
        /// </summary>
        /// <param name="postion">the localPosition to set</param>
        /// <param name="rotation">the localRotation to set</param>
        [Command(channel = 1)]
        public void CmdTransform(Vector3 postion, Quaternion rotation)
        {
            localPosition = postion;
            localRotation = rotation;
        }

        [SyncVar(hook = "AnchorEstablishedChanged")]
        bool AnchorEstablished;

        [Command]
        private void CmdSendAnchorEstablished(bool Established)
        {
            AnchorEstablished = Established;
            if (Established && SharesSpatialAnchors && !isLocalPlayer)
            {
                Debug.Log("remote device likes the anchor");
                anchorManager.AnchorFoundRemotely();
            }
        }

        void AnchorEstablishedChanged(bool update)
        {
            Debug.LogFormat("AnchorEstablished for {0} was {1} is now {2}", PlayerName, AnchorEstablished, update);
            AnchorEstablished = update;
            GetComponentInChildren<MeshRenderer>().enabled = update;
            if (!isLocalPlayer)
            {
                InitializeRemoteAvatar();
            }
        }

        [Command]
        private void CmdSetAnchorOwnerIP(string UpdatedIP)
        {
            anchorManager.UpdateAnchorOwnerIP(UpdatedIP);
        }

        public void SetAnchorOwnerIP(string UpdatedIP)
        {
            CmdSetAnchorOwnerIP(UpdatedIP);
        }

        [SyncVar(hook = "PlayerNameChanged")]
        string PlayerName;

        [Command]
        private void CmdSetPlayerName(string playerName)
        {
            PlayerName = playerName;
        }

        void PlayerNameChanged(string update)
        {
            Debug.LogFormat("Player name changing from {0} to {1}", PlayerName, update);
            PlayerName = update;
            if (PlayerName.ToLower() == "spectatorviewpc")
            {
                gameObject.SetActive(false);
            }
            else if (!isLocalPlayer)
            {
                InitializeRemoteAvatar();
            }
        }

#pragma warning disable 0414
        [SyncVar(hook = "PlayerIpChanged")]
        public string PlayerIp;
#pragma warning restore 0414
        [Command]
        private void CmdSetPlayerIp(string playerIp)
        {
            PlayerIp = playerIp;
        }

        void PlayerIpChanged(string update)
        {
            PlayerIp = update;
        }

        LineRenderer lineRend;

        [SyncVar(hook = "SharesAnchorsChanged")]
        public bool SharesSpatialAnchors;

        [Command]
        private void CmdSetCanShareAnchors(bool canShareAnchors)
        {
            Debug.Log("CMDSetCanShare " + canShareAnchors);
            SharesSpatialAnchors = canShareAnchors;
        }

        void SharesAnchorsChanged(bool update)
        {
            SharesSpatialAnchors = update;
            if (SharesSpatialAnchors)
            {
                cloudMaterial.color = Color.red;
            }
            else
            {
                cloudMaterial.color = Color.blue;
            }

            Debug.LogFormat("{0} {1} share", PlayerName, SharesSpatialAnchors ? "does" : "does not");
        }

        [Command]
        private void CmdUpdateAnchorName(string UpdatedName)
        {
            anchorManager.AnchorName = UpdatedName;
        }

        public void UpdateAnchorName(string UpdatedName)
        {
            CmdUpdateAnchorName(UpdatedName);
        }

        LevelControl levelState;

        NetworkDiscoveryWithAnchors networkDiscovery;

        private Material cloudMaterial;

        public bool AllowManualImmersionControl = false;

        private void InitializeRemoteAvatar()
        {
            if (!string.IsNullOrEmpty(PlayerName) && AnchorEstablished)
            {
                levelState.RemoteAvatarReady(this.gameObject, PlayerName, AnchorEstablished);
            }
        }

       
        void Awake()
        {
            cloudMaterial = GetComponentInChildren<MeshRenderer>().material;
            networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
            anchorManager = UNetAnchorManager.Instance;
            levelState = LevelControl.Instance;
            allPlayers.Add(this);
        }

        private void Start()
        {
            if (SharedCollection.Instance == null)
            {
                Debug.LogError("This script required a SharedCollection script attached to a gameobject in the scene");
                Destroy(this);
                return;
            }

            if (isLocalPlayer)
            {
                Debug.Log("Init from start");
                InitializeLocalPlayer();
            }
            else
            {
                Debug.Log("remote player, analyzing start state " + PlayerName);
                AnchorEstablishedChanged(AnchorEstablished);
                SharesAnchorsChanged(SharesSpatialAnchors);
            }

            sharedWorldAnchorTransform = SharedCollection.Instance.gameObject.transform;
            transform.SetParent(sharedWorldAnchorTransform);
        }

        private void InitializeLocalPlayer()
        {
            if (isLocalPlayer)
            {
                Debug.Log("Setting instance for local player ");
                _Instance = this;
                Debug.LogFormat("Set local player name {0} ip {1}", networkDiscovery.broadcastData, networkDiscovery.LocalIp);
                CmdSetPlayerName(networkDiscovery.broadcastData);
                CmdSetPlayerIp(networkDiscovery.LocalIp);
                bool opaqueDisplay = UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque;
                Debug.LogFormat("local player {0} share anchors ", (opaqueDisplay ? "does not" : "does"));
                CmdSetCanShareAnchors(!opaqueDisplay);

                if (opaqueDisplay && levelState != null && levelState.isActiveAndEnabled)
                {
                    Debug.Log("Requesting immersive path");
                }
                else
                {
                    Debug.Log("Defaulting to bird's eye view");
                    if (opaqueDisplay)
                    {
                         MixedRealityTeleport warper = MixedRealityTeleport.Instance;
                         if (warper != null)
                         {
                              //warper.ResetRotation();
                              warper.SetWorldPosition(levelState.transform.position + levelState.transform.forward * -2.5f + Vector3.up * 0.25f + levelState.transform.transform.right * Random.Range(-2f, 2.0f));
                         }
                    }
                }

                if (!opaqueDisplay && anchorManager.AnchorOwnerIP == "")
                {
                    Invoke("DeferredAnchorOwnerCheck", 2.0f);
                    
                }
            }
        }

        private void DeferredAnchorOwnerCheck()
        {
            if (!UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque && anchorManager.AnchorOwnerIP == "")
            {
                Debug.Log("Claiming anchor ownership " + networkDiscovery.LocalIp);
                CmdSetAnchorOwnerIP(networkDiscovery.LocalIp);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        private void OnDestroy()
        {
            if (allPlayers.Contains(this))
            {
                allPlayers.Remove(this);
            }

            if (levelState != null)
            {
                levelState.RemoteAvatarReady(this.gameObject, PlayerName, false);
            }

            if (cloudMaterial != null)
            {
                Destroy(cloudMaterial);
            }

            // Anchor owner is disconnecting, find a new anchor.
            if (UNetAnchorManager.Instance.AnchorOwnerIP == PlayerIp)
            {
                Debug.Log("Hey, the anchor owner is going away");
                anchorManager.AnchorOwnerIP = "";
            }
        }

        private void Update()
        {
            // If we aren't the local player, we just need to make sure that the position of this object is set properly
            // so that we properly render their avatar in our world.
            if (!isLocalPlayer && string.IsNullOrEmpty(PlayerName) == false)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, 0.3f);
                transform.localRotation = localRotation;
                return;
            }

            if (!isLocalPlayer)
            {
                return;
            }

            if (AnchorEstablished != anchorManager.AnchorEstablished)
            {
                CmdSendAnchorEstablished(anchorManager.AnchorEstablished);
                AnchorEstablished = anchorManager.AnchorEstablished;
            }

            if (AnchorEstablished == false)
            {
                return;
            }

            // if we are the remote player then we need to update our worldPosition and then set our 
            // local (to the shared world anchor) position for other clients to update our position in their world.
            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;

            // For UNET we use a command to signal the host to update our local position
            // and rotation
            CmdTransform(transform.localPosition, transform.localRotation);
        }

        private Vector3 Randomize(Vector3 newVector, float devation)
        {
            newVector += new Vector3(Random.Range(-6.0f, 6.0f), Random.Range(-6.0f, 6.0f), Random.Range(-6.0f, 6.0f)) * devation;
            newVector.Normalize();
            return newVector;
        }

        /// <summary>
        /// Called when the local player starts.  In general the side effect should not be noticed
        /// as the players' avatar is always rendered on top of their head.
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            GetComponentInChildren<MeshRenderer>().enabled = false;
        }

        [Command]
        private void CmdSendSharedTransform(GameObject target, Vector3 pos, Quaternion rot)
        {
            UNetSharedHologram ush = target.GetComponent<UNetSharedHologram>();
            ush.CmdTransform(pos, rot);
        }

        /// <summary>
        /// For sending transforms for holograms which do not frequently change.
        /// </summary>
        /// <param name="target">The shared hologram (must have a </param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public void SendSharedTransform(GameObject target, Vector3 pos, Quaternion rot)
        {
            if (isLocalPlayer)
            {
                CmdSendSharedTransform(target, pos, rot);
            }
        }

        [Command(channel = 1)]
        private void CmdSendImmersedPosition(string PlayerName, Vector3 levelPosition, Quaternion levelRotation)
        {
            levelState.SetRemoteAvatarLevelPosition(PlayerName, levelPosition, levelRotation);
        }

        public void SendImmersedPosition(Vector3 levelPosition, Quaternion levelRotation)
        {
            if (isLocalPlayer)
            {
                CmdSendImmersedPosition(this.PlayerName, levelPosition, levelRotation);
            }
        }
    }
}

#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
