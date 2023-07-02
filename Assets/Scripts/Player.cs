using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class Player : NetworkBehaviour
{
    public enum Team
    {
        Unassigned,
        Chaser,
        Hider,
        Tagged
    }

    public Color chaserColour;
    public Color hiderColour;
    public Color taggedColour;

    float canTagCooldown = 1.0f;
    float canTagCooldownTimer = 0.0f;

    public NetworkVariable<Team> team = new NetworkVariable<Team>(Team.Unassigned);
    public NetworkVariable<bool> canTag = new NetworkVariable<bool>(true);

    public AudioListener listener;

    public AudioSource onPlayerTaggedAudio;

    public override void OnNetworkSpawn()
    {
        team.OnValueChanged += OnTeamChanged;

        if (GameObject.Find("PlaceholderAudioListener") is GameObject go)
            Destroy(go);
        if (!IsOwner)
        {
            Destroy(gameObject.GetComponent<PlayerController>());
            Destroy(listener.gameObject);
        }
        if (!IsServer)
            return;
        GameManager.instance.OnPlayerSpawned(this);
    }

    public void OnTeamChanged(Team previous, Team next)
    {
        Color newColour = GetTeamColour(next);
        this.GetComponentInChildren<MeshRenderer>().material.color = newColour;
    }

    Color GetTeamColour(Team team)
    {
        switch (team)
        {
            case Team.Chaser:
                return chaserColour;
            case Team.Hider:
                return hiderColour;
            case Team.Tagged:
                return taggedColour;
            case Team.Unassigned:
            default:
                return Color.white;
        }
    }

    public void Update()
    {
        if (IsServer)
        {
            if (canTag.Value == false)
                return;

            canTagCooldownTimer += Time.deltaTime;
            if (canTagCooldownTimer > canTagCooldown)
            {
                canTagCooldownTimer = 0;
                canTag.Value = true;
            }
        }
        if (!IsClient)
            return;

        if (listener != null)
        {
            Vector3 forward = transform.position + Camera.main.transform.forward;
            forward.y = transform.position.y;
            //QueuedGizmos.DrawQueue.Add(QueuedGizmos.DrawSphere.Create(forward, 1.0f, Color.red), false);
            //QueuedGizmos.DrawQueue.Add(QueuedGizmos.DrawSphere.Create(transform.position, 1.0f, Color.green), false);
            //QueuedGizmos.DrawQueue.Add(QueuedGizmos.DrawSphere.Create(Camera.main.transform.forward, 1.0f, Color.blue), false);
            listener.gameObject.transform.LookAt(forward);
        }

        if (team.Value == Team.Chaser && canTag.Value)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 forward = (transform.position - Camera.main.transform.position);
                forward.y = 0;
                forward.Normalize();

                Vector3 pos = transform.position + (forward * 2);
                var target = GetUniquePlayerInRange(pos, 3.0f).Where(x => x != this).OrderBy(x => Vector3.Distance(x.transform.position, pos)).FirstOrDefault();
                Debug.Log($"PlayerFound:{target != null}");
                if (target != null)
                {
                    target.OnPlayerTagServerRpc();
                }
            }
        }
    }

    public HashSet<Player> GetUniquePlayerInRange(Vector3 position, float radius)
    {
        HashSet<Player> uniquePlayers = new HashSet<Player>();

        QueuedGizmos.DrawQueue.Add(QueuedGizmos.DrawSphere.Create(position, radius, Color.red), true);
        
        var colliders = Physics.OverlapSphere(position, radius, LayerMask.GetMask("Players"));
        foreach (var cc in colliders)
        {
            Debug.Log($"cc: {cc.gameObject.name}");
        }

        // Used to loop only once for each player.
        uniquePlayers.Add(this);
        foreach (var collider in colliders)
        {
            // If the object has a component and hasn't been found before.
            if (collider.gameObject.GetComponent<Player>() is Player player && !uniquePlayers.Contains(player))
            {
                uniquePlayers.Add(player);
            }
        }

        foreach (var up in uniquePlayers)
        {
            Debug.Log(up.name);
        }
        return uniquePlayers;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerTagServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
            return;
        var client = NetworkManager.ConnectedClients[clientId];
        if (canTag.Value)
        {
            Debug.Log("Attempting to tag!");
            if (!(client.PlayerObject.GetComponent<Player>() is Player tagger))
                return;
            if (Vector3.Distance(transform.position, tagger.transform.position) < 5.0f)
                this.team.Value = Team.Tagged;
            var go = Instantiate(onPlayerTaggedAudio);
            go.transform.position = transform.position;
            tagger.canTag.Value = false;
        }
    }
}
