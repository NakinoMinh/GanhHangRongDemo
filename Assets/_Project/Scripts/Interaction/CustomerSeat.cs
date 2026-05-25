using UnityEngine;
using GanhHangRong.Core;

namespace GanhHangRong.Interaction
{
    /// <summary>
    /// Ghế nhựa cho khách ngồi. 
    /// Nếu có khách thì Player không tương tác được. Nếu trống, Player có thể ngồi nghỉ.
    /// </summary>
    public class CustomerSeat : Interactable
    {
        private bool isOccupied = false;
        public bool IsOccupied => isOccupied;

        public void OccupySeat()
        {
            isOccupied = true;
            canInteract = false;
        }

        public void FreeSeat()
        {
            isOccupied = false;
            canInteract = true;
        }

        protected override void OnInteract(Player.PlayerController2D player)
        {
            if (isOccupied) return;

            // Player ngồi nghỉ
            if (player.CurrentState != PlayerState.Sitting)
            {
                player.transform.position = transform.position;
                player.SetState(PlayerState.Sitting);
                promptText = "Nhấn E để đứng lên";
            }
            else
            {
                player.SetState(PlayerState.Idle);
                promptText = "Ngồi nghỉ";
            }
        }
    }
}
