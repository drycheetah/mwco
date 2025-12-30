using System;
using System.Reflection;
using UnityEngine;

namespace MWCO.Client
{
    /// <summary>
    /// Loader class for DLL injection
    /// This class gets called by the injector
    /// </summary>
    public class Loader
    {
        /// <summary>
        /// Entry point for injection
        /// Called by injector (e.g., Unity Doorstop, BepInEx, etc.)
        /// </summary>
        public static void Load()
        {
            try
            {
                Debug.Log("[MWCO] Loader.Load() called");
                MWCOMod.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Loader failed: {ex.Message}");
                Debug.LogError($"[MWCO] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Unload method
        /// </summary>
        public static void Unload()
        {
            try
            {
                Debug.Log("[MWCO] Loader.Unload() called");
                MWCOMod.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MWCO] Unloader failed: {ex.Message}");
            }
        }
    }
}
