using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionRegion : MonoBehaviour
{
    protected GameObject client;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.transform.GetComponent<ClientNetworkTransform>().IsOwner)
                client = other.transform.gameObject;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.transform == client.transform)
        {
            client = null;
        }
    }

    public bool HasClient()
    {
        return client != null;
    }
}
