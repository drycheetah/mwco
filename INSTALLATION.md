# MWCO Installation Guide

## Quick Start

### Prerequisites

- My Winter Car (Steam version)
- BepInEx 5.x (Unity mod loader) - **Required for DLL injection**
- .NET Runtime (for running the server)

## Server Setup

### Running the Server

1. **Build the server** (if not already built):
   ```bash
   cd /path/to/mwco
   dotnet build MWCO.slnx
   ```

2. **Run the server**:
   ```bash
   cd MWCO.Server/MWCO.Server
   dotnet run
   ```

   Or specify a custom port:
   ```bash
   dotnet run -- 1999
   ```

3. The server will start listening on UDP port 1999 (default)

### Server Output
```
=================================
  MWCO Server - My Winter Car Online
=================================

[MWCO Server] Starting on port 1999...
[MWCO Server] Protocol version: 1
[MWCO Server] Physics tick rate: 50Hz
[MWCO Server] Receive loop started.
[MWCO Server] Tick loop started.
```

## Client Setup (Game Mod)

### Step 1: Install BepInEx

1. **Download BepInEx 5.x** for Unity (IL2CPP or Mono, depending on your game version):
   - https://github.com/BepInEx/BepInEx/releases

2. **Extract BepInEx** to your My Winter Car game folder:
   ```
   ~/.local/share/Steam/steamapps/common/My Winter Car/
   ```

3. **Run the game once** to let BepInEx generate its folders

4. You should now have these folders:
   ```
   My Winter Car/
   ├── BepInEx/
   │   ├── plugins/
   │   ├── core/
   │   └── config/
   └── mywintercar.exe / mywintercar
   ```

### Step 2: Install MWCO Client Mod

1. **Copy the built DLLs** to BepInEx plugins folder:
   ```bash
   cp MWCO.Client/bin/Debug/netstandard2.1/MWCO.Client.dll \
      ~/.local/share/Steam/steamapps/common/My\ Winter\ Car/BepInEx/plugins/

   cp MWCO.Shared/bin/Debug/netstandard2.1/MWCO.Shared.dll \
      ~/.local/share/Steam/steamapps/common/My\ Winter\ Car/BepInEx/plugins/

   cp MWCO.Client/bin/Debug/netstandard2.1/0Harmony.dll \
      ~/.local/share/Steam/steamapps/common/My\ Winter\ Car/BepInEx/plugins/
   ```

2. **Verify installation**:
   ```bash
   ls ~/.local/share/Steam/steamapps/common/My\ Winter\ Car/BepInEx/plugins/
   ```

   You should see:
   - `MWCO.Client.dll`
   - `MWCO.Shared.dll`
   - `0Harmony.dll`

### Step 3: Create BepInEx Plugin Loader

Create a file `~/.local/share/Steam/steamapps/common/My Winter Car/BepInEx/plugins/MWCOLoader.cs`:

```csharp
using BepInEx;
using UnityEngine;

namespace MWCO.BepInEx
{
    [BepInPlugin("com.mwco.multiplayer", "MWCO", "0.1.0")]
    public class MWCOPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo("MWCO Plugin loading...");
            MWCO.Client.MWCOMod.Initialize();
        }

        void OnDestroy()
        {
            MWCO.Client.MWCOMod.Shutdown();
        }
    }
}
```

## Usage

### Starting a Multiplayer Session

1. **Start the server** on your machine or a dedicated server:
   ```bash
   dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj
   ```

2. **Launch My Winter Car** with BepInEx installed

3. **In-game**:
   - Press **F10** to open the MWCO connection menu
   - Enter server address (e.g., `127.0.0.1` for local or server IP)
   - Enter port (`1999` by default)
   - Enter your player name
   - Click **Connect**

4. **Other players** can connect to the same server to join your session

### Controls

- **F10** - Toggle MWCO connection menu
- All normal My Winter Car controls work as usual
- Your inputs are sent to the server and other players see your vehicle

## Current Features (v0.1.0)

### Working:
- ✅ Server-client architecture
- ✅ UDP networking (port 1999)
- ✅ Connection/disconnection
- ✅ Vehicle spawn/despawn
- ✅ Basic transform synchronization
- ✅ Input capture from player's car
- ✅ Remote vehicle visualization
- ✅ Interpolation for smooth remote vehicles
- ✅ Wheel state synchronization
- ✅ Engine state (RPM, gear)
- ✅ Vehicle events (gear changes, engine start/stop)

### In Progress:
- ⏳ Server-side physics simulation
- ⏳ Client-side prediction
- ⏳ Part assembly/disassembly sync
- ⏳ World object synchronization (NPCs, items)
- ⏳ Damage synchronization
- ⏳ Fuel consumption sync
- ⏳ Time/weather synchronization

## Troubleshooting

### "MWCO not loading"
- Check BepInEx console output (`~/.local/share/Steam/steamapps/common/My Winter Car/BepInEx/LogOutput.log`)
- Ensure all DLLs are in the plugins folder
- Verify BepInEx is properly installed (run game once without mod first)

### "Cannot connect to server"
- Ensure server is running (`netstat -an | grep 1999`)
- Check firewall settings (allow UDP port 1999)
- Verify server address is correct

### "Remote vehicles not appearing"
- Check server console for connection messages
- Ensure both clients are connected to the same server
- Check for errors in Unity log (`~/.config/unity3d/Amistech/My Winter Car/Player.log`)

### "Game crashes on launch"
- Remove MWCO DLLs temporarily to isolate the issue
- Check BepInEx compatibility
- Ensure Unity and Assembly-CSharp.dll references are correct

## Network Protocol

### Packet Types
- **Connection** - Request, Accept, Deny, Disconnect, Heartbeat
- **Vehicle State** - Transform, physics, input (50Hz)
- **Wheel State** - Angular velocity, compression, steering (20Hz)
- **Vehicle Config** - Fuel, tire pressure, damage (5Hz)
- **Events** - Gear changes, engine, lights, horn
- **Parts** - Attach/detach synchronization
- **World** - Object sync, time/weather

### Bandwidth Usage (estimated)
- **Per client** (50Hz): ~4-6 KB/s upload, ~10-15 KB/s download (with 2+ players)
- **Server** (per client): ~10-15 KB/s per connected player

## Development

### Building from Source

```bash
# Clone/navigate to MWCO directory
cd /path/to/mwco

# Build all projects
dotnet build MWCO.slnx

# Run server
dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj

# Client DLL will be in:
# MWCO.Client/bin/Debug/netstandard2.1/MWCO.Client.dll
```

### Project Structure

```
mwco/
├── MWCO.Shared/          # Shared networking protocol
│   ├── NetworkConfig.cs
│   ├── PacketType.cs
│   └── Packets/          # All packet structures
├── MWCO.Server/          # Dedicated server
│   └── MWCO.Server/
│       ├── Program.cs
│       └── UdpServer.cs
├── MWCO.Client/          # Unity mod DLL
│   ├── MWCOMod.cs        # Main mod entry
│   ├── Loader.cs         # Injection loader
│   ├── Networking/       # Network manager, vehicle controllers
│   ├── Patches/          # Harmony patches
│   └── UI/               # In-game UI
└── decompiled/           # Decompiled game code (for reference)
```

## Next Steps

See [README.md](README.md) for:
- Detailed architecture
- Game code analysis
- Network protocol design
- Development roadmap

## Support

- **Issues**: https://github.com/your-repo/mwco/issues
- **Discord**: (coming soon)
- **Logs**: Check BepInEx logs and Unity Player.log for errors

---

**MWCO v0.1.0** - My Winter Car Online
