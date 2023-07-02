using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkAudio : NetworkBehaviour
{
    [SerializeField]
    private float life;

    [SerializeField]
    AudioSource audio;

    public void Awake()
    {
        life = audio.clip.length;
    }

    private void Update()
    {
        if (!IsServer)
            return;
        life -= Time.deltaTime;
        if (life <= 0)
            destroyAudioClientRpc();
    }

    [ClientRpc]
    void destroyAudioClientRpc()
    {
        Destroy(gameObject);
    }
}
