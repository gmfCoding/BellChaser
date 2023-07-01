using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerTest : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Destroy(gameObject.GetComponent<PlayerController>());
        }
        else
            GameObject.FindObjectOfType<FollowPlayer>().target = this.gameObject;
    }
}
