using System;
using HarmonyLib;
using UnityEngine;
using MWCO.Client.Networking;

namespace MWCO.Client.Patches
{
    /// <summary>
    /// Patches for CarController to capture player input
    /// </summary>
    [HarmonyPatch(typeof(CarController))]
    public class CarControllerPatches
    {
        /// <summary>
        /// Patch FixedUpdate to capture input state
        /// This runs every physics tick (50Hz)
        /// </summary>
        [HarmonyPatch("FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_Postfix(CarController __instance)
        {
            try
            {
                // Only process for player-controlled car (AxisCarController)
                if (!(__instance is AxisCarController))
                    return;

                var networkManager = NetworkManager.Instance;
                if (networkManager == null || !networkManager.IsConnected)
                    return;

                // Input is already captured by LocalVehicleController
                // This patch is just for monitoring/debugging if needed
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error in CarController.FixedUpdate patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Patch Update to capture input changes
        /// </summary>
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void Update_Postfix(CarController __instance)
        {
            try
            {
                if (!(__instance is AxisCarController))
                    return;

                var networkManager = NetworkManager.Instance;
                if (networkManager == null || !networkManager.IsConnected)
                    return;

                // Can add input event detection here if needed
                // For example, detecting gear shifts, horn presses, etc.
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Error in CarController.Update patch: {ex.Message}");
            }
        }
    }
}
