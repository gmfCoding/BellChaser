using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkDoor : NetworkBehaviour
{
    [System.Flags]
    enum DoorState
    {
        Closed = 0,
        Opened = 1,
        Moving = 2,
        Opening = 4 | Moving,
        Closing = 8 | Moving
    }

    public Animation doorAnimation;

    public string closeAnimation = "nd_undefined_animation";
    public string openAnimation = "nd_undefined_animation";

    public bool autoclose = false;
    public float closeTimer = 10.0f;
    private float timer = 0.0f;

    [SerializeField]
    private bool canClose = true;

    [SerializeField]
    private bool canOpen = true;

    [SerializeField]
    private bool startsOpen = false;
    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);
    DoorState state;

    public override void OnNetworkSpawn()
    {
        if (startsOpen)
        {
            doorAnimation.Play(openAnimation);
            doorAnimation[openAnimation].time = doorAnimation[openAnimation].clip.length;
            state = DoorState.Opened;
        }
        if (!IsServer)
            return;
        isOpen.Value = startsOpen;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void requestOpenServerRpc()
    {
        if (!canOpen || isOpen.Value == true || state.HasFlag(DoorState.Moving))
            return;
        state = DoorState.Opening;
        timer = 0.0f;
        isOpen.Value = true;
        requestOpenClientRpc();
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void requestCloseServerRpc()
    {
        if (!canClose || isOpen.Value == false || state.HasFlag(DoorState.Moving))
            return;
        state = DoorState.Closing;
        timer = 0.0f;
        isOpen.Value = false;
        requestCloseClientRpc();
    }

    public void Update()
    {
        if (state.HasFlag(DoorState.Moving))
        {
            timer += Time.deltaTime;
            if (state.HasFlag(DoorState.Opening) && timer > doorAnimation[openAnimation].length)
            { state = DoorState.Opened; timer = 0.0f; }
            else if (state.HasFlag(DoorState.Closing) && timer > doorAnimation[closeAnimation].length)
            { state = DoorState.Closed; timer = 0.0f; }
        }
        else if (IsServer && isOpen.Value && autoclose)  // SERVER ONLY: Autoclose the door, only if it's currently openned and autoclose is enabled
        {
            timer += Time.deltaTime;
            if (timer >= closeTimer)
            {
                timer = 0.0f;
                isOpen.Value = false;
                // Since we're sever side we can call requestCloseClientRpc to close the door on all clients.
                requestCloseClientRpc();
            }
        }
    }

    [ClientRpc]
    private void requestOpenClientRpc()
    {
        if (!canOpen)
            return;
        state = DoorState.Opening;
        doorAnimation.Play(openAnimation);
    }

    [ClientRpc]
    private void requestCloseClientRpc()
    {
        if (!canClose)
            return;
        state = DoorState.Closing;
        doorAnimation.Play(closeAnimation);
    }
}
