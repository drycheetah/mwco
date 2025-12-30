using System;
using UnityEngine;
using MWCO.Shared.Packets;

namespace MWCO.Client.Networking
{
    /// <summary>
    /// Controls the local player's vehicle
    /// Captures input and physics state to send to server
    /// </summary>
    public class LocalVehicleController : MonoBehaviour
    {
        public ushort VehicleId { get; private set; }
        public bool IsReady { get; private set; }

        // References to game components
        private CarController carController;
        private CarDynamics carDynamics;
        private Drivetrain drivetrain;
        private Axles axles;
        private Rigidbody rigidbody;
        private Transform vehicleTransform;

        // Cached state
        private Vector3 lastPosition;
        private Quaternion lastRotation;

        public void Initialize(ushort vehicleId)
        {
            VehicleId = vehicleId;
            Debug.Log($"[MWCO] LocalVehicleController initialized for vehicle {vehicleId}");

            // Find the player's car
            FindPlayerVehicle();
        }

        void Update()
        {
            if (!IsReady)
            {
                // Keep trying to find the vehicle
                if (Time.frameCount % 60 == 0) // Check every 60 frames
                {
                    FindPlayerVehicle();
                }
            }
        }

        private void FindPlayerVehicle()
        {
            // Try to find objects with Car tag or CarDynamics component
            var carDynamicsObjects = FindObjectsOfType<CarDynamics>();

            foreach (var cd in carDynamicsObjects)
            {
                // Check if this is the player's car (not AI)
                var controller = cd.GetComponent<CarController>();
                if (controller != null && controller is AxisCarController)
                {
                    // Found player car
                    AttachToVehicle(cd.gameObject);
                    return;
                }
            }

            Debug.LogWarning("[MWCO] Player vehicle not found yet...");
        }

        private void AttachToVehicle(GameObject vehicle)
        {
            vehicleTransform = vehicle.transform;
            rigidbody = vehicle.GetComponent<Rigidbody>();
            carController = vehicle.GetComponent<CarController>();
            carDynamics = vehicle.GetComponent<CarDynamics>();
            drivetrain = vehicle.GetComponent<Drivetrain>();
            axles = vehicle.GetComponent<Axles>();

            if (carController == null || carDynamics == null || drivetrain == null)
            {
                Debug.LogError("[MWCO] Failed to get required components from vehicle!");
                return;
            }

            IsReady = true;
            Debug.Log($"[MWCO] Attached to player vehicle: {vehicle.name}");
        }

        public VehicleInputPacket GetInputPacket(uint tick)
        {
            if (!IsReady)
                return new VehicleInputPacket(VehicleId, tick);

            var packet = new VehicleInputPacket(VehicleId, tick)
            {
                SteerInput = carController.steerInput,
                ThrottleInput = carController.throttleInput,
                BrakeInput = carController.brakeInput,
                HandbrakeInput = carController.handbrakeInput,
                ClutchInput = carController.clutchInput,
                TargetGear = (sbyte)drivetrain.gear
            };

            // Set input flags
            packet.SetFlag(VehicleInputPacket.FLAG_START_ENGINE, carController.startEngineInput);

            return packet;
        }

        public VehicleStatePacket GetStatePacket(uint tick)
        {
            if (!IsReady)
                return new VehicleStatePacket(VehicleId, tick);

            var packet = new VehicleStatePacket(VehicleId, tick);

            // Transform
            packet.PositionX = vehicleTransform.position.x;
            packet.PositionY = vehicleTransform.position.y;
            packet.PositionZ = vehicleTransform.position.z;

            packet.RotationX = vehicleTransform.rotation.x;
            packet.RotationY = vehicleTransform.rotation.y;
            packet.RotationZ = vehicleTransform.rotation.z;
            packet.RotationW = vehicleTransform.rotation.w;

            if (rigidbody != null)
            {
                packet.VelocityX = rigidbody.velocity.x;
                packet.VelocityY = rigidbody.velocity.y;
                packet.VelocityZ = rigidbody.velocity.z;

                packet.AngularVelocityX = rigidbody.angularVelocity.x;
                packet.AngularVelocityY = rigidbody.angularVelocity.y;
                packet.AngularVelocityZ = rigidbody.angularVelocity.z;
            }

            // Engine state
            packet.RPM = drivetrain.rpm;
            packet.Gear = (sbyte)drivetrain.gear;
            packet.EngineRunning = (byte)(drivetrain.rpm > drivetrain.minRPM ? 1 : 0);

            // Current processed inputs
            packet.Steering = carController.steering;
            packet.Throttle = carController.throttle;
            packet.Brake = carController.brake;

            return packet;
        }

        public WheelStatePacket GetWheelStatePacket(uint tick)
        {
            if (!IsReady || axles == null)
                return new WheelStatePacket(VehicleId, tick);

            var packet = new WheelStatePacket(VehicleId, tick);

            // Front left
            if (axles.frontAxle.leftWheel != null)
            {
                packet.FrontLeft = GetWheelData(axles.frontAxle.leftWheel);
            }

            // Front right
            if (axles.frontAxle.rightWheel != null)
            {
                packet.FrontRight = GetWheelData(axles.frontAxle.rightWheel);
            }

            // Rear left
            if (axles.rearAxle.leftWheel != null)
            {
                packet.RearLeft = GetWheelData(axles.rearAxle.leftWheel);
            }

            // Rear right
            if (axles.rearAxle.rightWheel != null)
            {
                packet.RearRight = GetWheelData(axles.rearAxle.rightWheel);
            }

            return packet;
        }

        private WheelData GetWheelData(Wheel wheel)
        {
            return new WheelData
            {
                AngularVelocity = wheel.angularVelocity,
                Compression = wheel.compression,
                SteeringAngle = wheel.steering,
                OnGround = (byte)(wheel.onGroundDown ? 1 : 0),
                TirePuncture = (byte)(wheel.tirePuncture ? 1 : 0)
            };
        }

        public VehicleConfigPacket GetConfigPacket(uint tick)
        {
            if (!IsReady)
                return new VehicleConfigPacket(VehicleId, tick);

            var packet = new VehicleConfigPacket(VehicleId, tick);

            // Fuel
            if (drivetrain.fuelTanks != null && drivetrain.fuelTanks.Length > 0)
            {
                packet.FuelLevel = drivetrain.fuelTanks[0].currentFuel;
            }
            packet.FuelConsumption = drivetrain.currentConsumption;

            // Tire pressures
            if (axles != null)
            {
                if (axles.frontAxle.leftWheel != null)
                    packet.TirePressureFL = axles.frontAxle.leftWheel.pressure;
                if (axles.frontAxle.rightWheel != null)
                    packet.TirePressureFR = axles.frontAxle.rightWheel.pressure;
                if (axles.rearAxle.leftWheel != null)
                    packet.TirePressureRL = axles.rearAxle.leftWheel.pressure;
                if (axles.rearAxle.rightWheel != null)
                    packet.TirePressureRR = axles.rearAxle.rightWheel.pressure;
            }

            // TODO: Get actual damage values
            packet.EngineDamage = 0f;
            packet.BodyDamage = 0f;

            return packet;
        }
    }
}
