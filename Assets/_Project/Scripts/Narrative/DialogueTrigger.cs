using UnityEngine;

namespace GanhHangRong.Narrative
{
    public class DialogueTrigger : Interaction.Interactable
    {
        [SerializeField] private DialogueData dialogue;
        [SerializeField] private bool oneShot = true;
        
        private bool hasTriggered = false;

        protected override void OnInteract(Player.PlayerController2D player)
        {
            if (hasTriggered && oneShot) return;

            if (dialogue != null)
            {
                DialogueManager.Instance.StartDialogue(dialogue);
                hasTriggered = true;
                if (oneShot) canInteract = false;
            }
        }
        
        // Cũng có thể kích hoạt qua Collider Trigger thay vì Interact
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered && oneShot) return;

            if (other.CompareTag(Core.Constants.TAG_PLAYER))
            {
                if (dialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(dialogue);
                    hasTriggered = true;
                    if (oneShot) canInteract = false;
                }
            }
        }
    }
}
