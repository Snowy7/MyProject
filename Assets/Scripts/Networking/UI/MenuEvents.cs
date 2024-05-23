using System;
using System.Collections.Generic;
using Fusion;
using Menu;
using Menu.Attribute;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Networking.UI
{
    // Loading Disposal
    public class Loader : IDisposable
    {
        public Loader(string message = "Loading...")
        {
            if (LoadingPanel.Instance)
            {
                LoadingPanel.Instance.Show(message);
            }
        }

        public void Dispose()
        {
            if (LoadingPanel.Instance)
            {
                LoadingPanel.Instance.Hide();
            }
        }
        
        public void SetMessage(string message)
        {
            if (LoadingPanel.Instance)
            {
                LoadingPanel.Instance.Show(message);
            }
        }
    }
    
    public class MenuEvents : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_Text usernameError;
        [SerializeField] private UIRoomHandler roomUI;

        [Title("Menu")]
        [SerializeField, MenuIndex] private int signinMenuIndex;
        
        [Title("Browser")]
        [SerializeField] private Transform container;
        [SerializeField] private RoomItem roomItemPrefab;
        
        [Title("Events")]
        public UnityEvent<SessionInfo> onJoinSuccess;
        public UnityEvent onJoinError;
        public UnityEvent onLeaveSuccess;

        private async void Start()
        {
            LobbyManager.OnFirstSessionListUpdate += UpdateSessionList;
            
            // Connect to a lobby
            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found");
                return;
            }
            
            
            MenuManager.Instance.Hide();
            using (new Loader("Connecting..."))
            {
                await LobbyManager.Instance.ConnectToLobbies();
                
                // Check if signed in.
                if (!LobbyManager.Instance.IsSignedIn)
                {
                    Debug.LogError("Not signed in");
                    MenuManager.Instance.OpenMenu(signinMenuIndex, false, false);
                    MenuManager.Instance.Show();
                }
                
                MenuManager.Instance.Show();
            }
        }
        
        private void OnDestroy()
        {
            LobbyManager.OnFirstSessionListUpdate -= UpdateSessionList;
        }

        private void OnDisable()
        {
            if (LobbyManager.Instance != null)
            {
                LobbyManager.OnFirstSessionListUpdate -= UpdateSessionList;
            }
        }

        public void UpdateSessionList(List<SessionInfo> sessions)
        {
            Debug.Log("Updating session list");
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            foreach (var session in sessions)
            {
                var roomItem = Instantiate(roomItemPrefab, container);
                roomItem.SetUp(session);
            }
        }

        public async void QuickJoin()
        {
            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found");
                return;
            }

            MenuManager.Instance.Hide();
            using (new Loader("Joining..."))
            {
                var isSuccess = await LobbyManager.Instance.QuickJoin();
                if (isSuccess)
                {
                    onJoinSuccess.Invoke(LobbyManager.Instance.CurrentSession);
                }
                else
                {
                    onJoinError.Invoke();
                    MenuManager.Instance.Show();
                }
            }
        }
        
        public void Refresh()
        {
            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found");
                return;
            }

            UpdateSessionList(LobbyManager.Instance.GetSessionList());
        }

        public async void Leave()
        {
            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found");
                return;
            }

            MenuManager.Instance.Hide();
            using (new Loader("Leaving..."))
            {
                var isSuccess = await LobbyManager.Instance.Leave();
                if (!isSuccess)
                {
                    MenuManager.Instance.Show();
                }
                else
                {
                    onLeaveSuccess.Invoke();
                }
            }
        }

        public void SignIn()
        {
            if (LobbyManager.Instance == null)
            {
                usernameError.text = "Error: LobbyManager not found";
                Debug.LogError("LobbyManager not found");
                return;
            }

            var username = usernameInput.text;
            if (string.IsNullOrEmpty(username))
            {
                usernameError.text = "Error: Username is empty";
                return;
            }
            
            if (username.Length < 3)
            {
                usernameError.text = "Error: Username is too short (min 3 characters)";
                return;
            }

            LobbyManager.Instance.SetUsername(username);
            
            MenuManager.Instance.OpenMenu(0, false, false);
        }
        
        public void StartGame()
        {
            LobbyManager.Instance.StartGame();
        }
    }
}