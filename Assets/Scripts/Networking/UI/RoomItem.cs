using System.Threading.Tasks;
using Fusion;
using Menu;
using TMPro;
using UnityEngine;

namespace Networking.UI
{
    public class RoomItem : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private TMP_Text roomName;
        [SerializeField] private TMP_Text playerCount;
        [SerializeField] private SnButton joinButton;
        
        [Title("Data")]
        [SerializeField] int roomMenuIndex;
        
        SessionInfo info;
        
        public void SetUp(SessionInfo sessionInfo)
        {
            roomName.text = sessionInfo.Name;
            playerCount.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";
            
            // if the room is full, disable the join button
            joinButton.SetIntractable(sessionInfo.PlayerCount < sessionInfo.MaxPlayers);
            
            info = sessionInfo;
        }
        
        public async void Join()
        {
            if (LobbyManager.Instance == null)
            {
                Debug.LogError("LobbyManager not found");
                return;
            }

            MenuManager.Instance.Hide();
            using (new Loader($"Joining {info.Name}...")) 
            {
                var isSuccess = await LobbyManager.Instance.JoinSession(info.Name);
                
                if (isSuccess)
                {
                    MenuManager.Instance.OpenMenuWithIndex(roomMenuIndex);
                }
                else
                {
                    MenuManager.Instance.Show();
                }
            }
        }
    }
}