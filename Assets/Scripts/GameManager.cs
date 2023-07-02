using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public static HashSet<Player> players = new ();

    public static HashSet<Player> unassigned = new();
    public static HashSet<Player> chasers = new ();
    public static HashSet<Player> hiders = new ();
    public static HashSet<Player> tagged = new ();

    bool doWarmUp = true;
    bool inWarmUp = true;

    public float warmUpPeriodTime = 30.0f;
    public float warmUpPeriodTimer = 0.0f;

    public float roundTime = 60.0f * 3;
    public float roundTimer = 0.0f;

    public int chasersCount = 2;
    public float maxChaserRatio = 0.34f;


    private void Start()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
    }
    public void OnNetworkSpawned()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
        if (!IsServer)
            Destroy(gameObject);
    }

    public void OnPlayerSpawned(Player player)
    {
        players.Add(player);
        unassigned.Add(player);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;

        if (players.Count <= 0)
            return;
        if (doWarmUp && inWarmUp)
        {
            warmUpPeriodTimer += Time.deltaTime;
            if (warmUpPeriodTimer >= warmUpPeriodTime)
            {
                inWarmUp = false;
                StartNewRound();
            }
        }
        else
        { 
            roundTimer += Time.deltaTime;

            if (roundTimer >= roundTime)
            {
                StartNewRound();
            }
        }
    }

    [ContextMenu("StartNewRound")]
    void StartNewRound()
    {
        chasers.Clear();
        hiders.Clear();
        tagged.Clear();
        unassigned = new HashSet<Player>(players);
        if (unassigned.Count <= 0)
            return;
        List<Player> randomPlayers = new List<Player>(unassigned);

        for (int i = 1; i <= chasersCount && randomPlayers.Count > 0; i++)
        {
            Player random = randomPlayers[Random.Range(0, randomPlayers.Count)];
            random.team.Value = Player.Team.Chaser;
            chasers.Add(random);
            randomPlayers.Remove(random);
            // We cannot have a ratio greater than maxChaserRatio, with two/three/four players there is still at least one chaser.
            if (i / players.Count > maxChaserRatio)
                break;
        }
        foreach (var item in randomPlayers)
        {
            item.team.Value = Player.Team.Hider;
            hiders.Add(item);
        }
    }
}
