# Peer Collaboration & Network Architecture Log
**Project:** 2-Player Lobby and Matchmaking System
**Engine:** Unity (with Mirror High-Level API)
**Transport:** KcpTransport
**Date:** September 2026

## 1. Tracking Client Readiness States
Our lobby system relies on Mirror’s `NetworkRoomManager` to orchestrate the transition between the offline state, the lobby staging area, and the synchronized gameplay scene. 

To track individual player readiness, each connecting peer is spawned with a `RoomPlayerPrefab` containing our custom `CustomRoomPlayer` script, which inherits from Mirror's `NetworkRoomPlayer`.
*   **State Synchronization:** The readiness state is stored in a boolean flag (`readyToBegin`) on the `NetworkRoomPlayer` component. This variable is managed by the server and synchronized to all clients via a `[SyncVar]`.
*   **State Modification:** Because clients do not have authority to arbitrarily change server variables, they must request a state change. When a player clicks the "READY" button in our 2D UI, the `LobbyUI` script accesses the local `CustomRoomPlayer` and executes `CmdChangeReadyState()`. This is a `[Command]` that travels from the client to the server, instructing the server to flip the player's readiness flag.
*   **Scene Transition:** The `NetworkRoomManager` listens to these state changes. Once the server confirms that `allPlayersReady` evaluates to true (and the `minPlayers` threshold is met), it invokes `ServerChangeScene("2_GameplayScene")`. This forces all connected peers to synchronously load the gameplay level, preventing race conditions where one player loads in before the other.

## 2. Handling Edge Cases & Disconnections
Handling disconnections gracefully is critical to preventing the lobby state machine from locking up.

**Scenario A: Client Exits the Lobby Before Start**
If Player 2 (the Client) closes their game window or manually disconnects while in `1_LobbyScene`, the KCP transport immediately recognizes the socket closure. 
*   The server automatically invokes `OnServerDisconnect`, which destroys Player 2's `RoomPlayer` GameObject across the network.
*   The server then recalculates the `allPlayersReady` condition. Even if Player 1 (the Host) is still locked in as "Ready", the lobby will halt the countdown and refuse to transition because the `minPlayers` requirement (2 players) is no longer satisfied. Player 1 will safely remain in the lobby waiting for a new connection.

**Scenario B: Host Disconnects**
Because our system utilizes a Listen-Server architecture (where Player 1 acts as both a local client and the authoritative server), the host dropping causes the entire session to terminate.
*   If the Host exits, the KcpServer socket is destroyed. 
*   Player 2's transport layer will detect a timeout/disconnect, triggering their local `OnClientDisconnect` callback. Mirror will then automatically transition Player 2 back to the `0_OfflineScene` so they can attempt to host or join a new room.

## 3. Peer Collaboration Notes
During development, the driver/navigator roles were utilized to systematically debug network hierarchy. Initial challenges involved race conditions where the UI buttons were unresponsive because the `EventSystem` was misconfigured due to a conflict with the New Input System. By collaborating, we successfully debugged the UI Raycasts, replaced the legacy StandaloneInputModule, and ensured the RPC commands (`CmdChangeReadyState`) successfully reached the server without errors.
