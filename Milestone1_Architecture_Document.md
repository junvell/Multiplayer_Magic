# Milestone 1 Architecture Document
**Project Title:** Project 3 - Synchronized Co-Op Projectile & Spell-Casting Pipeline
**Engine:** Unity (with Mirror High-Level API)
**Date:** September 2026

## 1. Project Title & Team Roles
This project implements an authoritative spell-casting system where multiple players connect to a local loopback server (or LAN) to cast energy projectiles. The pipeline focuses on enforcing server authority to prevent client cheating (e.g., cooldown circumvention, origin spoofing) and ensures precise Area-of-Effect (AoE) collision verification on the server.

**Team Roles & Responsibilities:**
*   **Student A (Unity Engine & Network Systems Lead):** Responsible for all Unity engine implementation, Mirror network architecture, custom `NetworkRoomManager` / `CustomRoomPlayer` staging lifecycle, RPC routing (`[Command]` / `[ClientRpc]`), authoritative server validations (cooldown enforcement, distance checks, `Physics.OverlapSphere` splash damage), and New Input System controls.
*   **Student B (3D Character Artist - Blender Lead):** Responsible for modeling, sculpting, UV unwrapping, and texturing the custom 3D **Dog** character (`Abarca_dayson_Kyusoru`) in Blender, managing pivot points, and configuring `.fbx` export settings for engine compatibility.
*   **Student C (3D Asset & Prop Artist - Blender Lead):** Responsible for 3D modeling, texturing, and asset preparation for the custom 3D **Cat** character (`AbarcaDayson_Cat`) and arena environment props in Blender, setting up mesh collider boundaries, and optimizing geometry for real-time networking.

## 2. Network Data Flow Diagram
The core pipeline for spell-casting ensures that all game-impacting logic executes strictly on the server, while clients handle input and visual feedback.

1.  **Input & Aiming (Client):** The `SpellCaster` component uses the New Input System to detect a left-click. It aims via a forward vector or raycast, triggers a local casting animation, and immediately dispatches `CmdCastSpell` to the server.
2.  **Server Verification (Server):** 
    *   *Cooldown Check:* Rejects requests if `Time.time` is less than `lastCastTime + spellCooldown`.
    *   *Origin Check:* Verifies the requested spawn origin is within a valid threshold of the player's actual `transform.position`.
3.  **Projectile Instantiation (Server):** The server instantiates the `NetworkedProjectile`, sets its trajectory, and invokes `NetworkServer.Spawn()` to synchronize the object to all clients.
4.  **Collision & AoE Resolution (Server):** When the projectile hits a collider, the server triggers `OnTriggerEnter`. It executes a `Physics.OverlapSphere` to locate any entities with a `NetworkHealth` component.
5.  **Damage & Feedback (Server to Clients):**
    *   The server calculates distance falloff damage and calls `TakeDamage()`. The `[SyncVar]` health automatically updates all clients.
    *   The server executes `RpcOnExplode()`, commanding all clients to instantiate visual explosion particles and audio at the impact point.
    *   The projectile is network-destroyed.

## 3. Connection Test Log & Guidelines
To fulfill Milestone 1 connectivity requirements, the prototype was tested using the following procedure:

**Procedure:**
1.  **Build & Run:** Compile the project via `File > Build Settings > Build and Run`.
2.  **Host Initialization:** In the standalone `.exe` window, click **Host Game** (acts as Listen-Server). The server binds to loopback (127.0.0.1) or the local LAN IP on port 7777.
3.  **Client Connection:** In the Unity Editor, press Play and click **Join Game**.
4.  **Lobby Verification:** Both clients successfully enter `1_LobbyScene`. The `LobbyUI` updates via `[SyncVar]` to reflect character selections (Dog vs Cat) and readiness states.
5.  **Synchronization:** Once both players lock in, `NetworkRoomManager` synchronously transitions both peers to `2_GameplayScene` where models, animations, and movement are fully networked with zero desynchronization.

### Verification Figures (Paste Screenshots Below):
*   **Figure 1: Lobby Handshake & Role/Character Selection:**
    *(Paste your screenshot showing both windows side-by-side in `1_LobbyScene` with Player 1 [Dog] and Player 2 [Cat] locked in as Ready)*

*   **Figure 2: Synchronized Scene Transition & Model Spawning:**
    *(Paste your screenshot showing both windows side-by-side in `2_GameplayScene` with both characters successfully spawned)*

*   **Figure 3: Real-Time Movement & Co-Op Interaction:**
    *(Paste your screenshot showing Player 1 moving/casting and Player 2 observing the motion in real-time with zero desync)*
