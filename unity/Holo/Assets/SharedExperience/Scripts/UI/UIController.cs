using HoloToolkit.Examples.SharingWithUNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.SharedExperience.Scripts.UI
{
    class UIController: MonoBehaviour
    {
        public GameObject ActivateOnStart;
        
        NetworkDiscoveryWithAnchors networkDiscovery;


        private void Start()
        {
            networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
        }

        public void StartSession()
        {
            if (networkDiscovery.running)
            {
                networkDiscovery.StartHosting("SuperRad");
            }
        }
        
        public void OfflineMode()
        {
            Debug.Log("Offline mode activated!");
            gameObject.SetActive(false);
            if (ActivateOnStart != null)
            {
                ActivateOnStart.SetActive(true);
            }
        }

        public void ScrollButton(int direction)
        {
            ScrollingSessionListUIController.Instance.ScrollSessions(direction);
        }

        public void ToogleSharing()
        {
            Debug.Log("Toggle sharing");
        }




    }
}
