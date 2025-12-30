using System;
using HarmonyLib;
using UnityEngine;
using MWCO.Client.Networking;
using MWCO.Shared.Packets;

namespace MWCO.Client.Patches
{
    /// <summary>
    /// Patches for Drivetrain to capture gear changes and engine events
    /// </summary>
    [HarmonyPatch(typeof(Drivetrain))]
    public class DrivetrainPatches
    {
        private static int lastGear = 0;
        private static bool lastEngineRunning = false;

        /// <summary>
        /// Patch Shift method to capture gear changes
        /// </summary>
        [HarmonyPatch("Shift")]
        [HarmonyPostfix]
        public static void Shift_Postfix(Drivetrain __instance, int m_gear)
        {
            try
            {
                // Check if this is the player's car
                var controller = __instance.GetComponent<CarController>();
                if (controller == null || !(controller is AxisCarController))
                    return;

                var networkManager = NetworkManager.Instance;
                if (networkManager == null || !networkManager.IsConnected)
                    return;

                // Gear changed, send event
                if (m_gear != lastGear)
                {
                    Debug.Log($"[MWCO] Gear changed to {m_gear}");
                    // TODO: Send gear change event packet
                    lastGear = m_gear;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error in Drivetrain.Shift patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Patch StartEngine to capture engine start
        /// </summary>
        [HarmonyPatch("StartEngine")]
        [HarmonyPostfix]
        public static void StartEngine_Postfix(Drivetrain __instance)
        {
            try
            {
                var controller = __instance.GetComponent<CarController>();
                if (controller == null || !(controller is AxisCarController))
                    return;

                var networkManager = NetworkManager.Instance;
                if (networkManager == null || !networkManager.IsConnected)
                    return;

                Debug.Log("[MWCO] Engine started");
                // TODO: Send engine start event
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error in Drivetrain.StartEngine patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Patch FixedUpdate to monitor engine state
        /// </summary>
        [HarmonyPatch("FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_Postfix(Drivetrain __instance)
        {
            try
            {
                var controller = __instance.GetComponent<CarController>();
                if (controller == null || !(controller is AxisCarController))
                    return;

                var networkManager = NetworkManager.Instance;
                if (networkManager == null || !networkManager.IsConnected)
                    return;

                // Check for engine stop
                bool engineRunning = __instance.rpm > __instance.minRPM;
                if (engineRunning != lastEngineRunning)
                {
                    if (!engineRunning)
                    {
                        Debug.Log("[MWCO] Engine stopped");
                        // TODO: Send engine stop event
                    }
                    lastEngineRunning = engineRunning;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error in Drivetrain.FixedUpdate patch: {ex.Message}");
            }
        }
    }
}
