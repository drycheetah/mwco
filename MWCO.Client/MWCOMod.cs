using System;
using HarmonyLib;
using UnityEngine;
using MWCO.Client.Networking;

namespace MWCO.Client
{
    /// <summary>
    /// Main entry point for MWCO mod
    /// This gets called when the DLL is injected
    /// </summary>
    public class MWCOMod
    {
        public static string ModVersion = "0.1.0";
        public static string HarmonyId = "com.mwco.multiplayer";

        private static GameObject networkManagerObject;
        private static Harmony harmony;

        /// <summary>
        /// Called when mod is loaded
        /// </summary>
        public static void Initialize()
        {
            try
            {
                Debug.Log($"[MWCO] Initializing My Winter Car Online v{ModVersion}");

                // Apply Harmony patches
                harmony = new Harmony(HarmonyId);
                harmony.PatchAll();
                Debug.Log("[MWCO] Harmony patches applied successfully");

                // Create NetworkManager GameObject
                networkManagerObject = new GameObject("MWCO_NetworkManager");
                networkManagerObject.AddComponent<NetworkManager>();
                networkManagerObject.AddComponent<UI.ConnectionUI>();
                UnityEngine.Object.DontDestroyOnLoad(networkManagerObject);

                Debug.Log("[MWCO] NetworkManager created");
                Debug.Log("[MWCO] MWCO initialized successfully!");
                Debug.Log("[MWCO] Press F10 to open connection menu");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Failed to initialize: {ex.Message}");
                Debug.LogError($"[MWCO] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Called when mod is unloaded
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                Debug.Log("[MWCO] Shutting down...");

                // Unpatch everything
                if (harmony != null)
                {
                    harmony.UnpatchAll(HarmonyId);
                    Debug.Log("[MWCO] Harmony patches removed");
                }

                // Destroy network manager
                if (networkManagerObject != null)
                {
                    UnityEngine.Object.Destroy(networkManagerObject);
                    Debug.Log("[MWCO] NetworkManager destroyed");
                }

                Debug.Log("[MWCO] Shutdown complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error during shutdown: {ex.Message}");
            }
        }
    }
}
