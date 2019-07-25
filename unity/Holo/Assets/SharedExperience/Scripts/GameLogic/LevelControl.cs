//#define LINE_REND 
// LINE_REND is for debugging the gaze ray.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Examples.SharingWithUNET;
using HoloToolkit.Unity.InputModule;
using System;
using HoloToolkit.Unity;

/// <summary>
/// This script has the primary game state logic for the Shared Mixed Reality 250 app.
/// </summary>
#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
public class LevelControl : NetworkBehaviour
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
{
    // Can't easily make network behaviors singletons with the template
    private static LevelControl _Instance;
    public static LevelControl Instance
    {
        get
        {
            LevelControl[] objects = FindObjectsOfType<LevelControl>();
            if (objects.Length != 1)
            {
                Debug.LogFormat("Expected exactly 1 {0} but found {1}", typeof(LevelControl).ToString(), objects.Length);
            }
            else
            {
                _Instance = objects[0];
            }
            return _Instance;
        }
    }

    /// <summary>
    /// Keeps track of information about other players in the scene
    /// </summary>
    public class LevelPlayerStateData
    {
        /// <summary>
        /// Indicates which path the user is on, or -1 if the user is not immersed
        /// </summary>
        private int _PathIndex = -1;
        public int PathIndex
        {
            get
            {
                return _PathIndex;
            }
            set
            {
                _PathIndex = value;
            }
        }

        /// <summary>
        /// Returns true if the user is immersed
        /// </summary>
        public bool Immersed
        {
            get
            {
                return _PathIndex >= 0;
            }
        }

        /// <summary>
        /// A gameobject with the script that has the playercontroller logic for the player
        /// </summary>
        public GameObject FullAvatar { get; set; }
        /// <summary>
        /// The avatar for the player while immersed or being viewed by an immersed player.
        /// </summary>
        public GameObject ImmersedAvatar { get; set; }
        /// <summary>
        /// An object to track the remote gaze cursor while immersed.
        /// </summary>
        public GameObject GazeIndicator { get; set; }
    }

    /// <summary>
    /// Wires up entry points for paths with a specific avatar
    /// puzzle, goal position, and other parameters that will be common
    /// </summary>
    [Serializable]
    public class ImmersedAvatarPathInfo
    {
        /// <summary>
        /// The start position for the path
        /// </summary>
        public GameObject EntryPoint;
        /// <summary>
        /// The Avatar to draw for the path
        /// </summary>
        public GameObject Avatar;
    }

    public bool EnableCollaboration = false;

    /// <summary>
    /// Describes how the paths work.  Configurable through the editor
    /// </summary>
    public ImmersedAvatarPathInfo[] AvatarStuff;

    /// <summary>
    /// How much larger the world should be when immersed.
    /// </summary>
    public const float ImmersiveScale = 128.0f;

    /// <summary>
    /// Tracks if we are immersed or not.
    /// </summary>
    public bool Immersed { get; private set; }

    /// <summary>
    /// Keeps track of the starting scale of the model.
    /// </summary>
    Vector3 startScale;

    /// <summary>
    /// Needed to track our parent so we can calculate the proper local postion
    /// </summary>
    public GameObject ParentObject;

    /// <summary>
    /// Object to instantiate for remote users' gaze cursors while immersed
    /// </summary>
    public GameObject GazeIndicatorPrefab;

    /// <summary>
    /// Object to spawn to represent remote users while immersed
    /// </summary>
    public GameObject GiantAvatar;

    /// <summary>
    /// Script which allows us to move the camera position independently from the HMD
    /// </summary>
    MixedRealityTeleport warper;

    /// <summary>
    /// When immersed we have some invisible colliders to prevent users from walking off the edges.
    /// </summary>
    public GameObject SafetyColliders;

    /// <summary>
    /// The local player controller object
    /// </summary>
    HoloToolkit.Examples.SharingWithUNET.PlayerController _PlayerController;
    HoloToolkit.Examples.SharingWithUNET.PlayerController playerController
    {
        get
        {
            if (_PlayerController == null)
            {
                _PlayerController = HoloToolkit.Examples.SharingWithUNET.PlayerController.Instance;
            }

            return _PlayerController;
        }
    }

    /// <summary>
    /// Keeps a mapping from user name to their game objects.
    /// </summary>
    Dictionary<string, LevelPlayerStateData> systemIdToPlayerState = new Dictionary<string, LevelPlayerStateData>();

    public Transform GetCurrentTransform(string PlayerName)
    {
        if (string.IsNullOrEmpty(PlayerName))
        {
            return null;
        }

        Transform retval = null;
        LevelPlayerStateData mad = null;
        if (systemIdToPlayerState.TryGetValue(PlayerName, out mad))
        {
            if (mad.Immersed)
            {
                retval = mad.ImmersedAvatar.transform;
            }
            else
            {
                retval = mad.FullAvatar.transform;
            }

        }

        return retval;
    }

    public bool IsImmersed(string PlayerName)
    {
        if (string.IsNullOrEmpty(PlayerName))
        {
            return false;
        }

        LevelPlayerStateData mad = null;
        if (systemIdToPlayerState.TryGetValue(PlayerName, out mad))
        {
            return (mad.Immersed);
        }

        return false;
    }

    /// <summary>
    /// The gaze beam was particularly tricky to implement correctly.  
    /// Enabling the LINE_REND enables drawing a line from the remote user's calculated position
    /// to the remote user's calculated gaze position. 
    /// </summary>
#if LINE_REND
    LineRenderer lineRend;
#endif

    public void LevelLocalTransformChanging(Vector3 old, Vector3 updated)
    {
        if (warper != null)
        {
            warper.SetWorldPosition(warper.transform.position + (updated - old) + Camera.main.transform.localPosition);
        }
    }

    void Start()
    {
#if LINE_REND
        // This is for debugging the gaze ray.
        lineRend = gameObject.AddComponent<LineRenderer>();
        lineRend.positionCount = 2;
        Vector3[] points = new Vector3[] { Vector3.zero, Vector3.one };
        lineRend.SetPositions(points);
#endif

        warper = MixedRealityTeleport.Instance;
        startScale = transform.localScale;
        SafetyColliders.SetActive(false);
        Debug.LogFormat("{0} {1}", gameObject.name, this.netId);
    }

    /// <summary>
    /// For the most part this script deals with how the player behaves while 
    /// immersed.
    /// </summary>
    void Update()
    {
        if (Immersed)
        {
            // Calculate the remote gaze vectors for other players
            DrawRemoteGaze();

            // calculate the rotation from our model rotation to our player rotation
            Quaternion rotToSend = Quaternion.Inverse(ParentObject.transform.rotation);
            rotToSend *= playerController.transform.localRotation;

            Vector3 modelLocalPosition = ParentObject.transform.InverseTransformPoint(playerController.transform.position);

            // Send our position relative to the model to other players
            playerController.SendImmersedPosition(modelLocalPosition, rotToSend);
        }
    }

    /// <summary>
    /// When an avatar is created for a user we need to do some bookkeeping for the path.
    /// When the user leaves we need to clean up.
    /// </summary>
    /// <param name="avatarObject">The full avatar object (with playercontrol script)</param>
    /// <param name="PlayerName">The player name according to the network</param>
    /// <param name="Created">indicates if the player was created or destroyed</param>
    public void RemoteAvatarReady(GameObject avatarObject, string PlayerName, bool Created)
    {
        if (string.IsNullOrEmpty(PlayerName))
        {
            return;
        }

        LevelPlayerStateData playerStateData;
        // First try to get the created avatar info the system is referencing
        if (systemIdToPlayerState.TryGetValue(PlayerName, out playerStateData) == false && Created)
        {
            // If we didn't find the avatar and we are being told that the user is being created 
            // we need to create the data.
            playerStateData = new LevelPlayerStateData();
            systemIdToPlayerState.Add(PlayerName, playerStateData);
            playerStateData.GazeIndicator = Instantiate(GazeIndicatorPrefab);
            Debug.Log("Created avatar for " + PlayerName);
            playerStateData.FullAvatar = avatarObject;
            playerStateData.GazeIndicator.SetActive(false);
            ConfigureAvatarsForPathState();
        }

        // if we found the avatar data and we are being told that the player is being removed
        // we need to clean up the avatar
        if (!Created && playerStateData != null)
        {
            Debug.Log("Cleaning up player avatar for " + PlayerName);
            if (playerStateData.ImmersedAvatar != null)
            {
                Destroy(playerStateData.ImmersedAvatar);
            }

            if (playerStateData.GazeIndicator != null)
            {
                Destroy(playerStateData.GazeIndicator);
            }

            playerStateData = null;
            systemIdToPlayerState.Remove(PlayerName);
        }
    }

    /// <summary>
    /// Positions a player in the immersed world
    /// </summary>
    /// <param name="PlayerName"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void SetRemoteAvatarLevelPosition(string PlayerName, Vector3 pos, Quaternion rot)
    {
        RpcSetRemoteAvatarLevelPosition(PlayerName, pos, rot);
    }

    /// <summary>
    /// Sent to all clients to sync a user's position while immersed
    /// </summary>
    /// <param name="PlayerName"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
#pragma warning disable CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    [ClientRpc]
#pragma warning restore CS0618 // using deprecated Unity stuff (TODO: upgrade in Holo project in the future)
    void RpcSetRemoteAvatarLevelPosition(string PlayerName, Vector3 pos, Quaternion rot)
    {
        LevelPlayerStateData lpsd;
        if (systemIdToPlayerState.TryGetValue(PlayerName, out lpsd) == false)
        {
            return;
        }

        if (lpsd.Immersed)
        {
            lpsd.ImmersedAvatar.transform.localPosition = pos;
            lpsd.ImmersedAvatar.transform.localRotation = rot;
        }
    }

    /// <summary>
    /// Calculates where a remote users's gaze cursor should be when a user is immersed 
    /// </summary>
    void DrawRemoteGaze()
    {
        foreach (KeyValuePair<string, LevelPlayerStateData> players in systemIdToPlayerState)
        {
            if (players.Value.Immersed == false && players.Value.GazeIndicator != null)
            {
                int noShadowboxMask = (1 << 30) | (1 << 31);
                noShadowboxMask = ~noShadowboxMask;
                // this transform is in the original space, but the model has been scaled ImmersiveScale times.
                // need to cast a ray from this transform's pov in our scaled up space.
                // 
                Transform remotePlayerTransform = players.Value.FullAvatar.transform;
                GazeStabilizer gazeStab = players.Value.GazeIndicator.GetComponent<GazeStabilizer>();
                if (gazeStab == null)
                {
                    gazeStab = players.Value.GazeIndicator.AddComponent<GazeStabilizer>();
                }
                gazeStab.UpdateStability(remotePlayerTransform.position, remotePlayerTransform.rotation);

                Vector3 remotePlayerToModel = gazeStab.StablePosition - transform.position;
                Vector3 remoteGazeOrigin = remotePlayerToModel * ImmersiveScale;


                Vector3 gazeTarget = remoteGazeOrigin + remotePlayerTransform.forward * ImmersiveScale * 2;
                Vector3 gazeNormal = Vector3.up;
                RaycastHit hitInfo;
                if (Physics.Raycast(remoteGazeOrigin, remotePlayerTransform.forward, out hitInfo, 1000.0f, noShadowboxMask))
                {
                    gazeTarget = hitInfo.point;
                    gazeNormal = hitInfo.normal;
                }

                players.Value.GazeIndicator.transform.position = Vector3.Lerp(players.Value.GazeIndicator.transform.position, gazeTarget, 0.1f);
                players.Value.GazeIndicator.transform.localRotation = Quaternion.Slerp(players.Value.GazeIndicator.transform.localRotation, Quaternion.FromToRotation(Vector3.up, gazeNormal), 0.5f);

                if (players.Value.ImmersedAvatar != null)
                {
                    players.Value.ImmersedAvatar.transform.position = remoteGazeOrigin;
                    players.Value.ImmersedAvatar.transform.LookAt(gazeTarget);
                }
                else
                {
                    Debug.Log("No immersed avatar...");
                }
#if LINE_REND
                Vector3[] points = new Vector3[] { remoteGazeOrigin, gazeTarget };
                lineRend.SetPositions(points);
#endif
            }
        }
    }

    /// <summary>
    /// Sets up the proper avatars for each remote player based on the local
    /// player's path index and the remote player's path index.
    /// </summary>
    void ConfigureAvatarsForPathState()
    {
        Debug.LogFormat("Configuring for path state we are {0} immersed", Immersed ? "in" : "not in");
        foreach (KeyValuePair<string, LevelPlayerStateData> kvp in systemIdToPlayerState)
        {
            Debug.LogFormat("Player {0} {1} immersed", kvp.Key, kvp.Value.Immersed ? "is" : "is not");
            // If the remote player is immersed we don't want to use their 'full' avatar, but want to use their
            // path dependent immersive avatar
            SetRenderersAndColliders(kvp.Value.FullAvatar, !Immersed);

            // we are immersed and the remote player is not
            if (Immersed && kvp.Value.Immersed == false)
            {
                // we might need to clean up their immersive avatar if they
                // have recently left the immersive state
                if (kvp.Value.ImmersedAvatar != null)
                {
                    Destroy(kvp.Value.ImmersedAvatar);
                    kvp.Value.ImmersedAvatar = null;
                }

                // and we need to setup the 'giant' avatar which represents their persona while
                // the local player is immersed
                kvp.Value.ImmersedAvatar = Instantiate(GiantAvatar);
            }

            // if we have an immersed avatar for the player we need to scale it based on if we are immersed.
            if (kvp.Value.ImmersedAvatar != null)
            {
                Vector3 defaultScale = kvp.Value.PathIndex >= 0 ? AvatarStuff[kvp.Value.PathIndex].Avatar.transform.localScale : GiantAvatar.transform.localScale;
                kvp.Value.ImmersedAvatar.transform.localScale = defaultScale * ((Immersed && kvp.Value.Immersed == false) ? ImmersiveScale * 0.25f : 1);
            }

            // Gaze should be enabled if we are immersed and the remote player is not.
            if (Immersed && kvp.Value.Immersed == false)
            {
                Debug.Log("Enabling gaze for " + kvp.Key);
                kvp.Value.GazeIndicator.SetActive(true);
            }
            else
            {
                Debug.Log("Disabling gaze for" + kvp.Key);
                kvp.Value.GazeIndicator.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Usually it's best to disable game objects to stop them from interacting, but sometimes
    /// we need to keep the game object active so update gets called.  This script will turn off 
    /// rendering and colliders for these objects.
    /// </summary>
    /// <param name="target">The target to 'disable' or 'enable'</param>
    /// <param name="enable">Whether to enable or disable the renderers/colliders</param>
    void SetRenderersAndColliders(GameObject target, bool enable)
    {
        MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in renderers)
        {
            meshRenderer.enabled = enable;
        }

        MeshCollider[] colliders = target.GetComponentsInChildren<MeshCollider>();
        foreach (MeshCollider meshCollider in colliders)
        {
            meshCollider.enabled = enable;
        }
    }

}
