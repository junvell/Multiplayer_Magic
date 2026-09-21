using Mirror;
using UnityEngine;

public class CustomRoomManager : NetworkRoomManager
{
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        CustomRoomPlayer cRoomPlayer = roomPlayer.GetComponent<CustomRoomPlayer>();
        PlayerMovement gamePlayerMovement = gamePlayer.GetComponent<PlayerMovement>();
        
        if (cRoomPlayer != null && gamePlayerMovement != null)
        {
            // Transfer the character index from the lobby player to the spawned game player
            gamePlayerMovement.characterIndex = cRoomPlayer.selectedCharacterIndex;
        }
        
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }
}

