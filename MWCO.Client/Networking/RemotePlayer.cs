using UnityEngine;
using System.Collections.Generic;

namespace MWCO.Client.Networking
{
    /// <summary>
    /// Represents another player in the world
    /// Handles rendering and interpolation of remote player state
    /// </summary>
    public class RemotePlayer : MonoBehaviour
    {
        public ushort PlayerId { get; private set; }
        public string PlayerName { get; private set; }

        private GameObject playerModel;
        private Transform playerTransform;

        // Interpolation
        private Queue<PlayerState> stateBuffer = new Queue<PlayerState>();
        private const float INTERPOLATION_DELAY = 0.1f; // 100ms
        private PlayerState? fromState;
        private PlayerState? toState;

        public void Initialize(ushort playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;

            // Create visual representation
            CreatePlayerModel();

            Debug.Log($"[MWCO] RemotePlayer created: {playerName} (ID: {playerId})");
        }

        private void CreatePlayerModel()
        {
            // Create a simple capsule for now (placeholder)
            playerModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerModel.name = $"RemotePlayer_{PlayerId}_{PlayerName}";
            playerModel.transform.SetParent(transform);
            playerModel.transform.localPosition = Vector3.zero;

            // Make it purple to distinguish from local player
            var renderer = playerModel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.7f, 0.3f, 0.9f); // Purple
            }

            // Add name tag above player
            CreateNameTag();

            playerTransform = playerModel.transform;
        }

        private void CreateNameTag()
        {
            // Create a simple text mesh for the name
            GameObject nameTagObj = new GameObject("NameTag");
            nameTagObj.transform.SetParent(playerModel.transform);
            nameTagObj.transform.localPosition = new Vector3(0, 1.2f, 0);

            TextMesh textMesh = nameTagObj.AddComponent<TextMesh>();
            textMesh.text = PlayerName;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            // Make it always face camera
            nameTagObj.AddComponent<Billboard>();
        }

        public void UpdateState(Vector3 position, Quaternion rotation)
        {
            PlayerState newState = new PlayerState
            {
                position = position,
                rotation = rotation,
                timestamp = Time.time
            };

            stateBuffer.Enqueue(newState);

            // Keep buffer from getting too large
            while (stateBuffer.Count > 10)
            {
                stateBuffer.Dequeue();
            }
        }

        void Update()
        {
            InterpolatePosition();
        }

        private void InterpolatePosition()
        {
            if (stateBuffer.Count == 0 || playerTransform == null)
                return;

            float renderTime = Time.time - INTERPOLATION_DELAY;

            // Find the two states to interpolate between
            while (stateBuffer.Count > 0)
            {
                PlayerState state = stateBuffer.Peek();

                if (state.timestamp > renderTime)
                {
                    // This state is in the future, use it as "to"
                    toState = state;
                    break;
                }
                else
                {
                    // This state is in the past, use as "from" and remove
                    fromState = stateBuffer.Dequeue();
                }
            }

            if (!fromState.HasValue || !toState.HasValue)
            {
                // Not enough states yet, just use the latest if available
                if (fromState.HasValue)
                {
                    playerTransform.position = fromState.Value.position;
                    playerTransform.rotation = fromState.Value.rotation;
                }
                return;
            }

            // Calculate interpolation factor
            float totalTime = toState.Value.timestamp - fromState.Value.timestamp;
            float t = totalTime > 0 ? (renderTime - fromState.Value.timestamp) / totalTime : 0f;
            t = Mathf.Clamp01(t);

            // Interpolate
            playerTransform.position = Vector3.Lerp(fromState.Value.position, toState.Value.position, t);
            playerTransform.rotation = Quaternion.Slerp(fromState.Value.rotation, toState.Value.rotation, t);
        }

        void OnDestroy()
        {
            if (playerModel != null)
            {
                Destroy(playerModel);
            }
        }

        private struct PlayerState
        {
            public Vector3 position;
            public Quaternion rotation;
            public float timestamp;
        }
    }

    /// <summary>
    /// Simple billboard component to make name tags face camera
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
