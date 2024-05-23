using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors.Online;
using Fusion;
using Networking;
using UnityEngine;

namespace Game
{
    public class TestGameManager : NetworkBehaviour, IAfterSpawned
    {
        public static TestGameManager Instance { get; private set; }
        
        [Title("References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private HoveringBall ballPrefab;
        [SerializeField, ReadOnly] private List<Player.Online.Player> players = new ();
        
        [Title("Settings")]
        [SerializeField] private bool canDamageOtherPlayers = true;
        [SerializeField] private int countDown = 3;
        
        HoveringBall m_ball;
        
        public bool CanDamageOtherPlayers => canDamageOtherPlayers;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void AfterSpawned()
        {
            if (HasStateAuthority) StartCoroutine(CountDown());
            RunnerEventHandler.Instance.PlayerJoined.AddListener(OnPlayerJoined);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            RunnerEventHandler.Instance.PlayerJoined.RemoveListener(OnPlayerJoined);
            base.Despawned(runner, hasState);
        }

        // This is a test game manager, it should spawn the players after a countdown
        private IEnumerator CountDown()
        {
            RPC_SpawnPlayers();

            for (int i = countDown; i > 0; i--)
            {
                Debug.Log($"Starting in {i}...");
                yield return new WaitForSeconds(1);
                
                if (i == 1)
                {
                    Debug.Log("Starting now!");
                }
            }
            
            // Spawn the ball
            if (m_ball)
            {
                Runner.Despawn(m_ball.GetBehaviour<NetworkObject>());
            }
            
            var ballSpawnPoint = SpawnManager.Instance.GetBallSpawnPoint();
            m_ball = Runner.Spawn(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // Spawn the player
            if (Runner.IsSceneAuthority)
            {
                int index = runner.ActivePlayers.Count() - 1;
                RPC_SpawnPlayer(player, index, player.PlayerId);
            }
        }
        
        public void SpawnPlayer(PlayerRef player, int index = -1)
        {
            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(index);
            Runner.Spawn(playerPrefab, spawnPoint.position, spawnPoint.rotation, player);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnPlayers()
        {
            int index = Runner.ActivePlayers.Select((value, index) => new {value, index}).Single(x => x.value == Runner.LocalPlayer).index;
            Debug.Log("Spawning player: " + index);
            
            SpawnPlayer(Runner.LocalPlayer, index);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnPlayer(PlayerRef player, int index, int playerID)
        {
            if (Runner.LocalPlayer.PlayerId == playerID) SpawnPlayer(player, index);
        }
        
        public void AddPlayer(Player.Online.Player player)
        {
            players.Add(player);
        }
        
        public void RemovePlayer(Player.Online.Player player)
        {
            players.Remove(player);
        }
        
        public List<Player.Online.Player> GetPlayers()
        {
            return players;
        }
        
        public void BallHit()
        {
            Debug.Log("Ball hit!");
            Reset();
        }

        public void Reset()
        {
            if (!Runner.IsSceneAuthority) return;
            // Despawn the ball
            if (m_ball)
            {
                Runner.Despawn(m_ball.GetBehaviour<NetworkObject>());
            }
            
            // Move the players to new spawn points
            foreach (var player in players)
            {
                var stats = player.GetStats();
                int index = SpawnManager.Instance.GetRandomSpawnPointIndex();
                
                RPC_SetPosition(stats.PlayerId, index);
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetPosition(int playerId, int index)
        {
            if (playerId != Runner.LocalPlayer.PlayerId) return;
            
            var player = players.FirstOrDefault(x => x.GetStats().PlayerId == playerId);
            if (player == null) return;
            
            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(index);
            player.SetPosition(spawnPoint.position);
        }
    }
}