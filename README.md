# MWCO - My Winter Car Online

My Winter Car Online is a multiplayer mod for My Winter Car, enabling multiple players to drive and interact in the same game world together.

## Architecture

MWCO uses a client-server architecture with UDP networking on port 1999.

### Components

- **MWCO.Shared** - Shared networking protocol, packet structures, and common code
- **MWCO.Server** - Dedicated server for hosting multiplayer sessions
- **MWCO.Client** - Unity mod that integrates with My Winter Car

### Network Protocol

The protocol is designed around the game's 50Hz physics tick rate with prioritized updates:

#### Update Rates
- **High Priority (50Hz)**: Vehicle transforms, inputs, engine state
- **Medium Priority (20Hz)**: Wheel states, suspension
- **Low Priority (5Hz)**: Fuel levels, damage, part configuration

#### Key Features
- **Client-side prediction** for responsive local player control
- **Server-authoritative physics** to prevent cheating
- **Interpolation/extrapolation** for smooth remote vehicle movement
- **Delta compression** to minimize bandwidth
- **Tick-based synchronization** for consistent simulation

## Game Analysis

### Decompiled Code Structure

My Winter Car's core systems (from decompiled `Assembly-CSharp.dll`):

#### Vehicle Control
- [CarController.cs](decompiled/CarController.cs) - Abstract input handler with TCS/ABS/ESP
- [AxisCarController](decompiled/AxisCarController.cs), [MouseCarController](decompiled/MouseCarController.cs), [MobileCarController](decompiled/MobileCarController.cs) - Input implementations

#### Physics Simulation
- [CarDynamics.cs](decompiled/CarDynamics.cs) - Physics coordinator, center of mass, weight distribution
- [Drivetrain.cs](decompiled/Drivetrain.cs) - Complex engine simulation with torque curves
- [Wheel.cs](decompiled/Wheel.cs) - Pacejka Magic Formula tire physics
- [Axles.cs](decompiled/Axles.cs) - Wheel management and anti-roll bars

#### Game State
- [StartGame.cs](decompiled/StartGame.cs) - Game initialization
- [UniqueSaveManager.cs](decompiled/UniqueSaveManager.cs) - ES2-based save system
- [Setup.cs](decompiled/Setup.cs) - Vehicle setup and configuration

### Network Synchronization Requirements

Based on the decompiled code analysis, here's what needs to be synchronized:

#### High Priority (Every Physics Tick - ~50Hz)

**Vehicle Transform:**
```
- Position (Vector3)
- Rotation (Quaternion)
- Velocity (Vector3)
- Angular velocity (Vector3)
```

**Vehicle Control State:**
```
- Steering input (float)
- Throttle input (float)
- Brake input (float)
- Handbrake (float)
- Clutch (float)
```

**Engine State:**
```
- RPM (float)
- Current gear (int)
- Engine running (bool)
```

#### Medium Priority (10-20Hz)

**Wheel State (per wheel x4):**
```
- Angular velocity (float)
- Suspension compression (float)
- Steering angle (float)
- Brake pressure (float)
```

#### Low Priority (1-5Hz)

**Vehicle Configuration:**
```
- Fuel level (float)
- Tire pressure (float x4)
- Damage state
- Part installation status
```

## Building

### Prerequisites
- .NET SDK 10.0 or later
- Unity-compatible .NET Standard 2.1 for client

### Build Commands

```bash
# Build entire solution
dotnet build MWCO.sln

# Build specific projects
dotnet build MWCO.Shared/MWCO.Shared.csproj
dotnet build MWCO.Server/MWCO.Server/MWCO.Server.csproj
dotnet build MWCO.Client/MWCO.Client.csproj

# Run server
dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj
```

## Packet Structures

### VehicleStatePacket (84 bytes)
High-frequency vehicle state broadcast from server to all clients.

Fields:
- Header (8 bytes)
- VehicleId (2 bytes)
- Transform data: position, rotation, velocity, angular velocity (48 bytes)
- Engine state: RPM, gear, running (10 bytes)
- Current inputs: steering, throttle, brake (12 bytes)

### VehicleInputPacket (34 bytes)
Client input commands sent to server.

Fields:
- Header (8 bytes)
- VehicleId (2 bytes)
- Raw inputs: steer, throttle, brake, handbrake, clutch (20 bytes)
- Input flags: start engine, shift, horn, lights (1 byte)
- Target gear (1 byte)

### ConnectionRequestPacket
Client connection request with player name and protocol version.

### ConnectionResponsePacket
Server response with assigned player/vehicle ID and acceptance status.

## Networking Challenges & Solutions

### 1. Complex Physics Simulation
**Challenge:** Pacejka tire model and custom physics make deterministic simulation difficult.

**Solution:** Server-authoritative physics with client-side prediction. Remote players use simplified interpolation.

### 2. Fixed Timestep Dependency
**Challenge:** Game runs at fixed 0.02s (50Hz) timestep.

**Solution:** Tick-based synchronization matching the physics rate.

### 3. Input Smoothing
**Challenge:** CarController applies extensive input smoothing (steerTime, throttleTime).

**Solution:** Send raw inputs from clients, apply smoothing on local player only. Server processes raw inputs.

### 4. State Explosion
**Challenge:** 100+ state variables per vehicle.

**Solution:**
- Prioritized update rates
- Delta compression
- Quantization for non-critical values
- Area of interest management

### 5. Gear Shifting & Clutch
**Challenge:** Complex automatic shifting logic is timing-sensitive.

**Solution:** Sync gear changes as discrete events. Send clutch position rather than simulating engagement.

## Development Roadmap

### Phase 1: Basic Transform Sync ✅
- [x] Analyze game code structure
- [x] Design network protocol
- [x] Create packet structures
- [x] Set up project structure
- [ ] Implement basic server
- [ ] Create Unity client mod
- [ ] Position/rotation interpolation

### Phase 2: Input Synchronization
- [ ] Replicate steering/throttle/brake
- [ ] Visual-only remote vehicles
- [ ] Client-side prediction

### Phase 3: Physics Synchronization
- [ ] Wheel states
- [ ] Engine/drivetrain state
- [ ] Server-authoritative collision

### Phase 4: Gameplay Features
- [ ] Part assembly/disassembly sync
- [ ] World object interaction
- [ ] Vehicle damage synchronization
- [ ] Fuel consumption sync

### Phase 5: Optimization
- [ ] Delta compression
- [ ] Interest management
- [ ] Bandwidth optimization
- [ ] Cheat prevention

## Technical Details

### Game Engine
- Unity (older version based on UnityEngine.dll)
- Custom physics (not using WheelCollider)
- PlayMaker for visual scripting
- ES2 for save system
- Master Audio for sound

### Physics Tick Rate
- Fixed timestep: 0.02s (50Hz)
- Critical for accurate simulation

### Tire Physics
- Pacejka Magic Formula
- Parameters in [TireParameters.cs](decompiled/TireParameters.cs)
- Tire types: competition, supersport, sport, touring, offroad, truck, winter

## Contributing

This is a work in progress! Contributions are welcome.

### Key Files to Modify
- Hook into `CarController.cs` for input capture
- Monitor `CarDynamics.cs` for physics state
- Track `Drivetrain.cs` for engine/gear state
- Intercept `Wheel.cs` for wheel physics

## License

TBD

## Credits

- My Winter Car by Amistech Games
- Decompilation via ILSpy
