using Mirror;
using UnityEngine;
using TMPro; // We use TextMeshPro for modern UI

public class LobbyUI : MonoBehaviour
{
    [Header("UI Assignments")]
    public TMP_Text player1Text;
    public TMP_Text player2Text;

    void Update()
    {
        // 1. Reset texts to default 'Waiting' state in case a player hasn't joined
        if (player1Text != null) 
        {
            player1Text.text = "Player 1: Waiting for connection...";
            player1Text.color = Color.gray;
        }
        
        if (player2Text != null) 
        {
            player2Text.text = "Player 2: Waiting for connection...";
            player2Text.color = Color.gray;
        }

        // 2. Find all active players in the Lobby scene
        CustomRoomPlayer[] players = FindObjectsByType<CustomRoomPlayer>(FindObjectsSortMode.None);
        
        // 3. Update the UI based on their status
        foreach (var player in players)
        {
            // Player index 0 is Host (Player 1), index 1 is Client (Player 2)
            TMP_Text targetText = (player.index == 0) ? player1Text : player2Text;
            
            if (targetText != null)
            {
                // Set the base color: Player 1 is Blue, Player 2 is Red
                targetText.color = (player.index == 0) ? Color.blue : Color.red;

                if (player.readyToBegin)
                {
                    targetText.text = $"Player {player.index + 1}: READY!";
                }
                else
                {
                    targetText.text = $"Player {player.index + 1}: Not Ready";
                }
            }
        }
    }

    public void ToggleReady()
    {
        // Find the local player object in the lobby
        if (NetworkClient.localPlayer != null)
        {
            var roomPlayer = NetworkClient.localPlayer.GetComponent<CustomRoomPlayer>();
            if (roomPlayer != null)
            {
                // Toggles the ready state
                roomPlayer.CmdChangeReadyState(!roomPlayer.readyToBegin);
            }
        }
    }
}