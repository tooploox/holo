using HoloToolkit.Examples.SharingWithUNET;
using UnityEngine;

namespace Assets.SharedExperience.Scripts.UI
{
    class UIController: MonoBehaviour
    {
        public GameObject ModelWithPlate;
        public GameObject DebugWindow;
        ScrollingSessionListUIController scrollingUIControl;
        NetworkDiscoveryWithAnchors networkDiscovery;


        private void Start()
        {
            networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
            scrollingUIControl = ScrollingSessionListUIController.Instance;
        }

        public void StartSession()
        {
            if (networkDiscovery.running)
            {
                MovePlateToFront();
                networkDiscovery.StartHosting("SuperRad");
                gameObject.SetActive(false);
            }
        }

        public void JoinSession()
        {
            MovePlateToFront();
            scrollingUIControl.JoinSelectedSession();
            gameObject.SetActive(false);
        }
        
        public void OfflineMode()
        {
            Debug.Log("Offline mode activated!");
            gameObject.SetActive(false);
            if (ModelWithPlate != null)
            {
                MovePlateToFront();
                ModelWithPlate.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        public void ToogleSharing()
        {
            Debug.Log("Toggle sharing");
        }

        private bool isDebugWindowActive = false;
        public void ToggleDebugWindow()
        {
            isDebugWindowActive = !isDebugWindowActive;
            DebugWindow.SetActive(isDebugWindowActive);
        }

        private void MovePlateToFront()
        {
            ModelWithPlate.transform.position = gameObject.transform.position + gameObject.transform.TransformDirection(new Vector3(0, 0, 3f));
            ModelWithPlate.transform.rotation = gameObject.transform.rotation;
        }
    }
}
