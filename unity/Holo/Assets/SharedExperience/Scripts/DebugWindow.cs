using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Examples.SharingWithUNET;

namespace MRTK.Tutorials.AzureSpatialAnchors
{
    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI debugText = default;

        Queue<string> logMessages = new Queue<string>();
        Queue<string> nextLogMessages = new Queue<string>();
        int maxLogMessages = 30;

        int frameCount;
        int framesPerSecond;
        int lastWholeTime = 0;
        NetworkDiscoveryWithAnchors networkDiscovery;
        UNetAnchorManager anchorManager;

        private ScrollRect scrollRect;

        private void Start()
        {
            // Cache references
            scrollRect = GetComponentInChildren<ScrollRect>();

            networkDiscovery = NetworkDiscoveryWithAnchors.Instance;

            // Subscribe to log message events
            Application.logMessageReceived += HandleLog;

            // Set the starting text
            debugText.text = "Debug messages will appear here.\n\n";
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            LogCallback(message, stackTrace, type);
        }

        private void LogCallback(string Message, string stack, LogType logType)
        {
            lock (logMessages)
            {
                while (logMessages.Count > maxLogMessages)
                {
                    logMessages.Dequeue();
                }

                logMessages.Enqueue(Message);
                if (logType == LogType.Exception)
                {
                    logMessages.Enqueue(stack);
                }
            }
        }

        void Update()
        {
            string logMessageString = "";

            lock (logMessages)
            {
                while (logMessages.Count > 0)
                {
                    string nextMessage = logMessages.Dequeue();
                    logMessageString += string.Format("{0}\n", nextMessage);
                    // for the next frame...
                    nextLogMessages.Enqueue(nextMessage);
                }

                Queue<string> tmp = logMessages;
                logMessages = nextLogMessages;
                nextLogMessages = tmp;
                nextLogMessages.Clear();
            }


            bool fire1 = Input.GetButton("Fire1"); // a
            bool fire2 = Input.GetButton("Fire2"); // b
            bool fire3 = Input.GetButton("Fire3"); // x
            bool jump = Input.GetButton("Jump"); // y

            string ButtonString = string.Format("Buttons: {0}{1}{2}{3}\nControllers: ", fire1 ? "O" : "X", fire2 ? "O" : "X", fire3 ? "O" : "X", jump ? "O" : "X");
            foreach (string str in Input.GetJoystickNames())
            {
                ButtonString += string.Format("{0}\n", str);
            }

            frameCount++;
            int currentWholeTime = (int)Time.realtimeSinceStartup;
            if (currentWholeTime != lastWholeTime)
            {
                lastWholeTime = currentWholeTime;
                framesPerSecond = frameCount;
                frameCount = 0;
            }

            ButtonString += string.Format("FPS: {0}\n", framesPerSecond);

            if (anchorManager == null)
            {
                anchorManager = UNetAnchorManager.Instance;
            }
            else
            {
                ButtonString += string.Format("{0}\n",
                    anchorManager.GenerateDebugString());
            }
            string NetworkInfoString = string.Format(
                "Port: {0}\nhostID: {1}\nrcv count: {2}\nIsClient? {3}\nis Server? {4}\nis Running?{5}\n",
                networkDiscovery.broadcastPort,
                networkDiscovery.hostId,
                networkDiscovery.broadcastsReceived == null ? 0 : networkDiscovery.broadcastsReceived.Count,
                networkDiscovery.isClient,
                networkDiscovery.isServer,
                networkDiscovery.running);

            debugText.text = string.Format("{0}\n{1}\n{2}\n", NetworkInfoString, ButtonString, logMessageString);

            debugText.color = new Color(fire1 ? 0 : 0xFF, fire2 ? 0 : 0xFF, fire3 ? 0 : 0xFF);
        }
    }
}
