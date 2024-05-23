using System;
using Fusion;
using TMPro;
using UnityEngine;

namespace Networking.UI
{
    public class UIRoomHandler : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private TMP_Text roomName;
        [SerializeField] private TMP_Text roomPlayers;
        [SerializeField] private GameObject startButton;
        
        private SessionInfo sessionInfo;
        
        public void Setup(SessionInfo info)
        {
            sessionInfo = info;
            RefreshUI();
        }

        private void OnEnable()
        {
            RunnerEventHandler.Instance.PlayerJoined.AddListener(OnJoin);
            RunnerEventHandler.Instance.PlayerLeft.AddListener(OnLeave);
        }

        private void OnDisable()
        {
            RunnerEventHandler.Instance.PlayerJoined.RemoveListener(OnJoin);
            RunnerEventHandler.Instance.PlayerLeft.RemoveListener(OnLeave);
            
            sessionInfo = null;
            roomName.text = "";
            roomPlayers.text = "";
        }

        private void RefreshUI()
        {
            if (roomName) roomName.text = sessionInfo.Name;
            if (roomPlayers) roomPlayers.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";
            
            if (startButton) startButton.SetActive(LobbyManager.Instance.Runner.IsServer || LobbyManager.Instance.Runner.IsSharedModeMasterClient);
        }

        private void OnJoin(NetworkRunner runner, PlayerRef player)
        {
            sessionInfo = runner.SessionInfo;
            RefreshUI();
        }
        
        private void OnLeave(NetworkRunner runner, PlayerRef player)
        {
            sessionInfo = runner.SessionInfo;
            RefreshUI();
        }
    }
}