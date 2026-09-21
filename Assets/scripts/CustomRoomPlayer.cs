using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    // This runs when the Client starts for this specific player object
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Player joined lobby. Index: {index}");
    }

    // Optional: Use this to update UI text to show if player is ready
    public void Update()
    {
        // For debugging purposes in the console
        if (Input.GetKeyDown(KeyCode.R) && isLocalPlayer)
        {
            ToggleReady();
        }
    }

    public void ToggleReady()
    {
        // This is a built-in Mirror function that syncs state
        CmdChangeReadyState(!readyToBegin);
    }

    // This is called by Mirror when the ready state changes
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        Debug.Log($"Player {index} Ready State: {newReadyState}");
    }
}