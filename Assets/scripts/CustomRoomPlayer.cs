using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public int selectedCharacterIndex = 0; // 0 = Dog, 1 = Cat

    // This runs when the Client starts for this specific player object
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Player joined lobby. Index: {index}");
    }

    // Update method removed to prevent legacy Input System crash

    public void ToggleReady()
    {
        // This is a built-in Mirror function that syncs state
        CmdChangeReadyState(!readyToBegin);
    }

    [Command]
    public void CmdSelectCharacter(int characterIndex)
    {
        if (readyToBegin) return; // Prevent changing character after locking in
        selectedCharacterIndex = characterIndex;
    }

    public void SwitchCharacter()
    {
        if (isLocalPlayer)
        {
            // Toggle between 0 (Dog) and 1 (Cat)
            int nextIndex = (selectedCharacterIndex + 1) % 2;
            CmdSelectCharacter(nextIndex);
        }
    }

    // This is called by Mirror when the ready state changes
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        Debug.Log($"Player {index} Ready State: {newReadyState}");
    }
}