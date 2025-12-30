using System;
using UnityEngine;
using MWCO.Client.Networking;

namespace MWCO.Client.UI
{
    /// <summary>
    /// Simple UI for connecting to MWCO servers
    /// </summary>
    public class ConnectionUI : MonoBehaviour
    {
        private bool showUI = true;
        private string serverAddress = "127.0.0.1";
        private string serverPort = "1999";
        private string playerName = "Player";
        private string statusMessage = "";

        private Rect windowRect = new Rect(20, 20, 300, 200);

        void Start()
        {
            playerName = SystemInfo.deviceName;
        }

        void Update()
        {
            // Toggle UI with F10
            if (Input.GetKeyDown(KeyCode.F10))
            {
                showUI = !showUI;
            }
        }

        void OnGUI()
        {
            if (!showUI)
                return;

            windowRect = GUI.Window(0, windowRect, DrawWindow, "MWCO - My Winter Car Online");
        }

        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            var networkManager = NetworkManager.Instance;
            bool isConnected = networkManager != null && networkManager.IsConnected;

            if (!isConnected)
            {
                // Connection form
                GUILayout.Label("Server Address:");
                serverAddress = GUILayout.TextField(serverAddress);

                GUILayout.Label("Port:");
                serverPort = GUILayout.TextField(serverPort);

                GUILayout.Label("Player Name:");
                playerName = GUILayout.TextField(playerName);

                GUILayout.Space(10);

                if (GUILayout.Button("Connect"))
                {
                    Connect();
                }
            }
            else
            {
                // Connected status
                GUILayout.Label($"Connected to {serverAddress}:{serverPort}");
                GUILayout.Label($"Player ID: {networkManager.LocalPlayerId}");
                GUILayout.Label($"Vehicle ID: {networkManager.LocalVehicleId}");

                GUILayout.Space(10);

                if (GUILayout.Button("Disconnect"))
                {
                    networkManager.Disconnect();
                    statusMessage = "Disconnected";
                }
            }

            // Status message
            if (!string.IsNullOrEmpty(statusMessage))
            {
                GUILayout.Space(10);
                GUILayout.Label(statusMessage);
            }

            GUILayout.Space(10);
            GUILayout.Label("Press F10 to toggle this window");

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void Connect()
        {
            try
            {
                if (!int.TryParse(serverPort, out int port))
                {
                    statusMessage = "Invalid port number";
                    return;
                }

                var networkManager = NetworkManager.Instance;
                if (networkManager == null)
                {
                    statusMessage = "NetworkManager not found";
                    return;
                }

                networkManager.Connect(serverAddress, port);
                statusMessage = "Connecting...";
            }
            catch (Exception ex)
            {
                statusMessage = $"Error: {ex.Message}";
                Debug.LogError($"[MWCO] Connection error: {ex.Message}");
            }
        }
    }
}
