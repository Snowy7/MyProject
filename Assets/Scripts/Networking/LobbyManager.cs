using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Networking
{
    public class LobbyManager : MonoBehaviour
    {
        
        [SerializeField] bool autoConnect = true;
        [SerializeField] NetworkRunner runnerPrefab;
        
        public static LobbyManager Instance { get; private set; }
        public SessionInfo CurrentSession => Runner.SessionInfo;
        public string Username { get; private set; } = "";
        public bool IsSignedIn => !string.IsNullOrEmpty(Username);

        public static event Action<List<SessionInfo>> OnFirstSessionListUpdate;
        public static event Action<List<SessionInfo>> OnSessionListUpdate;
        
        private NetworkRunner networkRunner;
        private List<SessionInfo> sessionList = new List<SessionInfo>();
        private bool firstLoad = true;

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

            DontDestroyOnLoad(gameObject);
            
            // Check if the user is signed in
            var user = PlayerPrefs.GetString("Username");
            if (!string.IsNullOrEmpty(user))
            {
                Username = user;
            }
        }
        
        private async void Start()
        {
            if (autoConnect)
            {
                await ConnectToLobbies();
            }
        }
        
        private void OnDisable()
        {
            if (networkRunner != null && RunnerEventHandler.Instance != null)
            {
                RunnerEventHandler.Instance.OnSessionListUpdate.RemoveListener(OnSessionListUpdated);
            }
        }
        
        private NetworkRunner AddRunner()
        {
            var runner = Instantiate(runnerPrefab);
            firstLoad = true;
            RunnerEventHandler.Instance.OnSessionListUpdate.AddListener(OnSessionListUpdated);
            return runner;
        }

        // Join Shared Mode sessions
        public async Task ConnectToLobbies()
        {
            if (!Runner) return;
            if (!Runner.LobbyInfo.IsValid)
            {
                await Runner.JoinSessionLobby(SessionLobby.Shared);
            } 
        }

        public async Task<bool> QuickJoin()
        {
            Debug.Log("Quick join");
            if (!Runner)
            {
                Debug.LogError("Runner not found");
                return false;
            }
            // Get a random session

            if (sessionList.Count > 0)
            {
                var session = sessionList[Random.Range(0, sessionList.Count)];
                return await JoinSession(session.Name);
            }

            var sessionName = $"{Username}'s Session";
            return await JoinSession(sessionName);
        }
        
        public async Task<bool> JoinSession(string sessionName)
        {
            Debug.Log("Quick join");
            var result = await Runner.StartGame(new StartGameArgs
            {
                SessionName = sessionName,
                GameMode = GameMode.Shared
            });
            
            if (!result.Ok)
            {
                Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            }

            return result.Ok;
        }

        public async Task<bool> Leave()
        {
            var result = await Runner.Shutdown().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to leave: {task.Exception}");
                    return false;
                }

                return true;
            });
            
            await ConnectToLobbies();
            
            return result;
        }
        
        public void SetUsername(string username)
        {
            Username = username;
            PlayerPrefs.SetString("Username", username);
        }
        
        public void StartGame()
        {
            if (Runner && Runner.IsSceneAuthority)
            {
                Runner.LoadScene(SceneRef.FromIndex(1));
            }
        }
        
        public void TestCall(string message)
        {
            Debug.Log($"Test call: {message}");
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessions)
        {
            Debug.Log("Updating session list");
            sessionList = sessions;
            if (firstLoad)
            {
                OnFirstSessionListUpdate?.Invoke(sessionList);
                firstLoad = false;
            }
            else
            {
                OnSessionListUpdate?.Invoke(sessionList);
            }
        }
        
        public List<SessionInfo> GetSessionList()
        {
            return sessionList;
        }
        
        public NetworkRunner Runner
        {
            get
            {
                if (!networkRunner)
                {
                    networkRunner = AddRunner();
                }
                
                return networkRunner;
            }
        }
    }
}