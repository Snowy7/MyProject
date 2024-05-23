using Fusion;
using Networking;

namespace Player.Online
{
    public class PlayerStats : NetworkBehaviour
    {
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public int PlayerId { get; set; }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                PlayerName = LobbyManager.Instance.Username;
                PlayerId = Runner.LocalPlayer.PlayerId;
            }
            base.Spawned();
        }
    }
}