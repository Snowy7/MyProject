using UnityEngine;

namespace Game
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }
        
        [Title("References")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform ballSpawnPoint;
        
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
        
        public Transform GetSpawnPoint(int index)
        {
            return spawnPoints[Mathf.Clamp(index, 0, spawnPoints.Length - 1)];
        }
        
        public Transform GetRandomSpawnPoint()
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        
        public int GetRandomSpawnPointIndex()
        {
            return Random.Range(0, spawnPoints.Length);
        }
        
        public Transform GetBallSpawnPoint()
        {
            return ballSpawnPoint;
        }
    }
}