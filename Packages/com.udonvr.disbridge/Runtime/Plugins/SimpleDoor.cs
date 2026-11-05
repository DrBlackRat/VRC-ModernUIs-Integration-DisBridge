//Copyright (c) 2025 UdonVR LLC - All Rights Reserved
using UdonSharp;
using UdonVR.DisBridge;
using UdonVR.DisBridge.Plugins;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

public enum DoorInteractMode
{
    Interact,
    PlayerEnter,
    PlayerLeave
}
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SimpleDoor : DisBridgePlugin
{
    public PlayerMover playerMover;
    public Transform doorSpawn;
    public DoorInteractMode doorInteractMode = DoorInteractMode.Interact;
    
    void Start()
    {
        DisableInteractive = doorInteractMode != DoorInteractMode.Interact;
    }

    public override void Interact()
    {
        if (doorInteractMode != DoorInteractMode.Interact) return;
        __MovePlayer();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if(!player.isLocal) return;
        if (doorInteractMode != DoorInteractMode.PlayerEnter) return;
        __MovePlayer();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if(!player.isLocal) return;
        if (doorInteractMode != DoorInteractMode.PlayerLeave) return;
        __MovePlayer();
    }

    private void __MovePlayer()
    {
        if (!__PermCheck()) return;
        if (playerMover)
        {
            playerMover._MovePlayer(doorSpawn);
        }
        else
        {
            Networking.LocalPlayer.TeleportTo(doorSpawn.position, doorSpawn.rotation);
        }
    }

    private bool __PermCheck()
    {
        if (disBridge == null) return true;
        return _IsMemberInRoles(Networking.LocalPlayer);
    }
}
