using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ElevatorPad : NetworkBehaviour
{
    [System.Flags]
    enum ElevatorState
    {
        Stopped = 0,
        Moving = 1,
        Up = 2 | Moving,
        Down = 4 | Moving
    }

    public bool autoFloor = false;
    public float autoFloorTime = 10.0f;
    private float timer = 0.0f;

    [SerializeField]
    private float speed = 3.0f;

    public float[] floors;

    [SerializeField]
    private bool canOperate = true;

    [SerializeField]
    private int startFloor = 0;

    private NetworkVariable<int> floor = new NetworkVariable<int>(0);
    ElevatorState state;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        floor.Value = startFloor;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    public void requestFloorServerRpc(int floorID)
    {
        if (!canOperate || floor.Value == floorID || state.HasFlag(ElevatorState.Moving))
            return;
        timer = 0.0f;
        floorID = Mathf.Clamp(floorID, 0, floors.Length - 1);
        state = floorID < floor.Value ? ElevatorState.Down : ElevatorState.Up;
        floor.Value = floorID;
        if (transform.localPosition.y - floors[floor.Value] > 0)
            gameObject.GetComponent<Rigidbody>().AddForce(-transform.up * speed, ForceMode.VelocityChange);
        else
            gameObject.GetComponent<Rigidbody>().AddForce(transform.up * speed, ForceMode.VelocityChange);
    }

    public void FixedUpdate()
    {
        if (!IsServer)
            return;
        if (state.HasFlag(ElevatorState.Stopped) && startFloor != floor.Value)
        {
            timer += Time.fixedDeltaTime;
            if (timer > autoFloorTime)
                requestFloorServerRpc(startFloor);
        }
        if (Mathf.Abs(transform.localPosition.y - floors[floor.Value]) > 0.01)
        {
            if (Mathf.Abs(transform.localPosition.y - floors[floor.Value]) < 3 * speed * Time.fixedDeltaTime)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, floors[floor.Value], transform.localPosition.z);
                state = ElevatorState.Stopped;
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                return;
            }

            //if (transform.localPosition.y - floors[floor.Value] < 0)
            //    transform.localPosition += transform.up * speed * Time.fixedDeltaTime;
            //else
            //    transform.localPosition -= transform.up * speed * Time.fixedDeltaTime;
        }
    }
}
