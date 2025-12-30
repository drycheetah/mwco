# MWCO Quick Start Guide

## The Fastest Way to Get Started

### Option 1: Use the Launcher (Recommended)

```bash
cd /path/to/mwco
./mwco-launcher.sh
```

The launcher provides a menu with:
- **[1] Start Server** - Launch the multiplayer server
- **[2] Install/Update Mod** - Auto-install mod to game
- **[3] Launch My Winter Car** - Start the game via Steam
- **[4] Quick Start** - Server + Game in one click
- **[5] Settings** - View configuration
- **[6] Help** - Documentation links

### Option 2: Manual Setup

#### Step 1: Install BepInEx
1. Download BepInEx 5.x from https://github.com/BepInEx/BepInEx/releases
2. Extract to: `~/.local/share/Steam/steamapps/common/My Winter Car/`
3. Run the game once to generate folders

#### Step 2: Install MWCO Mod
```bash
cd /path/to/mwco
./install-mod.sh
```

#### Step 3: Start Server
```bash
dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj
```

#### Step 4: Play
1. Launch My Winter Car
2. Press **F10** in-game
3. Enter: `127.0.0.1:1999`
4. Click Connect

## Testing Multiplayer

### Single Machine (2 Game Instances)

**Terminal 1 - Server:**
```bash
dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj
```

**Terminal 2 - Launch Game:**
```bash
steam steam://rungameid/516750
```

- Press F10, connect to `127.0.0.1:1999`
- Launch game again (2nd instance)
- Both players connect to same server
- You'll see each other's cars!

### LAN/Internet Play

**On Server Machine:**
```bash
# Find your IP
ip addr show | grep "inet "

# Start server
dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj
```

**On Client Machines:**
1. Launch My Winter Car
2. Press F10
3. Enter server IP: `<server_ip>:1999`
4. Connect

## What You Should See

### Server Output:
```
=================================
  MWCO Server - My Winter Car Online
=================================

[MWCO Server] Starting on port 1999...
[MWCO Server] Protocol version: 1
[MWCO Server] Physics tick rate: 50Hz
[MWCO Server] Receive loop started.
[MWCO Server] Tick loop started.
[MWCO Server] Connection request from <ip>, player: Player1
[MWCO Server] Client connected as Player1 (Player ID: 1, Vehicle ID: 1)
```

### Client (In-Game):
1. Press F10 → Connection menu appears
2. Enter server details
3. Click Connect
4. See "Connected! Player ID: X, Vehicle ID: X" in console
5. Drive around - your movements are synced!
6. Other players' cars appear as blue cubes (placeholder visuals)

## Troubleshooting

### "Mod not loading"
```bash
# Check BepInEx log
cat ~/.local/share/Steam/steamapps/common/My\ Winter\ Car/BepInEx/LogOutput.log | grep MWCO
```

### "Can't connect to server"
```bash
# Check if server is running
netstat -an | grep 1999

# Check firewall (if remote)
sudo ufw allow 1999/udp
```

### "No remote vehicles appearing"
- Ensure both clients connected successfully (check server console)
- Look for errors in Unity log:
  ```bash
  tail -f ~/.config/unity3d/Amistech/My\ Winter\ Car/Player.log | grep MWCO
  ```

## Features Currently Working

✅ **Connection Management**
- UDP networking on port 1999
- Multi-client connections
- Automatic reconnection handling

✅ **Vehicle Synchronization**
- Position, rotation, velocity (50Hz)
- Engine RPM, gear state
- Steering, throttle, brake inputs
- All 4 wheels (rotation, compression, steering)

✅ **Visual Representation**
- Remote players shown as blue cubes (placeholder)
- Smooth interpolation (100ms buffer)
- Wheel rotation visual

✅ **Events**
- Gear changes
- Engine start/stop
- (More coming soon)

## Next Steps

After you verify it works:

1. **Improve Remote Vehicle Visuals**
   - Currently placeholders (cubes)
   - Need to clone actual car models

2. **Server-Side Physics**
   - Currently just relays packets
   - Need authoritative physics simulation

3. **Part Synchronization**
   - Detect part attach/detach
   - Sync to other clients

4. **World Sync**
   - NPCs, AI cars
   - Physics objects

## Performance

**Network Usage Per Client:**
- Upload: ~4-6 KB/s
- Download: ~10-15 KB/s (with 2-3 players)

**Server:**
- CPU: Minimal (packet routing)
- Memory: ~50MB
- Network: ~10-15 KB/s per connected client

## Files to Check

**Logs:**
- BepInEx: `<Game>/BepInEx/LogOutput.log`
- Unity: `~/.config/unity3d/Amistech/My Winter Car/Player.log`
- Server: Console output

**Config:**
- Default port: 1999
- Protocol version: 1
- Physics tick: 50Hz

**Installation:**
- Mod DLLs: `<Game>/BepInEx/plugins/`
- Required files: `MWCO.Client.dll`, `MWCO.Shared.dll`, `0Harmony.dll`

## Support

For detailed info, see:
- [README.md](README.md) - Full documentation
- [INSTALLATION.md](INSTALLATION.md) - Detailed setup
- [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) - Technical details

---

**Have fun with My Winter Car Online!** 🚗💨
