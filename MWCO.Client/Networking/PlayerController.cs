using UnityEngine;
using MWCO.Shared;
using MWCO.Shared.Packets;

namespace MWCO.Client.Networking
{
    /// <summary>
    /// Manages local player state and sends updates to server
    /// Handles player position, rotation, animations, and interactions
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private Transform playerTransform;
        private Animator playerAnimator;
        private NetworkManager networkManager;

        // Player state
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private float sendTimer = 0f;
        private const float SEND_INTERVAL = 0.02f; // 50Hz

        // Player identification
        private string playerName;

        public void Initialize(string name)
        {
            playerName = name;
            networkManager = NetworkManager.Instance;

            // Find player GameObject - typically named "PLAYER" or "FPSController"
            GameObject playerObj = GameObject.Find("PLAYER") ?? GameObject.Find("FPSController");

            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerAnimator = playerObj.GetComponent<Animator>();

                lastPosition = playerTransform.position;
                lastRotation = playerTransform.rotation;

                Debug.Log($"[MWCO] PlayerController initialized for {playerName}");
            }
            else
            {
                Debug.LogError("[MWCO] Could not find player GameObject!");
            }
        }

        void Update()
        {
            if (playerTransform == null || networkManager == null || !networkManager.IsConnected)
                return;

            sendTimer += Time.deltaTime;

            if (sendTimer >= SEND_INTERVAL)
            {
                SendPlayerState();
                sendTimer = 0f;
            }
        }

        private void SendPlayerState()
        {
            // Check if position/rotation changed significantly
            if (Vector3.Distance(playerTransform.position, lastPosition) > 0.01f ||
                Quaternion.Angle(playerTransform.rotation, lastRotation) > 0.5f)
            {
                PlayerStatePacket packet = new PlayerStatePacket(
                    networkManager.LocalPlayerId,
                    networkManager.CurrentTick
                );

                packet.PositionX = playerTransform.position.x;
                packet.PositionY = playerTransform.position.y;
                packet.PositionZ = playerTransform.position.z;

                packet.RotationX = playerTransform.rotation.x;
                packet.RotationY = playerTransform.rotation.y;
                packet.RotationZ = playerTransform.rotation.z;
                packet.RotationW = playerTransform.rotation.w;

                // Detect animation states from velocity/input
                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float speed = rb.velocity.magnitude;
                    packet.IsWalking = (byte)(speed > 0.1f && speed < 3f ? 1 : 0);
                    packet.IsRunning = (byte)(speed >= 3f ? 1 : 0);
                }

                networkManager.SendPacket(packet);

                lastPosition = playerTransform.position;
                lastRotation = playerTransform.rotation;
            }
        }

        // Handle player animations
        public void UpdateAnimation(string animationName, bool state)
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetBool(animationName, state);
            }
        }

        // Handle player interactions
        public void OnPlayerInteract(GameObject interactedObject)
        {
            Debug.Log($"[MWCO] Player interacted with: {interactedObject.name}");

            // Send world object interaction packet
            WorldObjectPacket packet = new WorldObjectPacket(
                (uint)interactedObject.GetInstanceID(),
                interactedObject.name,
                1, // ObjectType: physics object
                PacketType.WorldObjectUpdate,
                networkManager.CurrentTick
            );

            packet.PosX = interactedObject.transform.position.x;
            packet.PosY = interactedObject.transform.position.y;
            packet.PosZ = interactedObject.transform.position.z;

            packet.RotX = interactedObject.transform.rotation.x;
            packet.RotY = interactedObject.transform.rotation.y;
            packet.RotZ = interactedObject.transform.rotation.z;
            packet.RotW = interactedObject.transform.rotation.w;

            Rigidbody rb = interactedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                packet.VelX = rb.velocity.x;
                packet.VelY = rb.velocity.y;
                packet.VelZ = rb.velocity.z;
            }

            networkManager.SendPacket(packet.ToBytes());
        }
    }
}
