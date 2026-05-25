using UnityEngine;
using GanhHangRong.Core;

namespace GanhHangRong.Player
{
    /// <summary>
    /// Quản lý animation thủ tục cho nhân vật.
    /// Dùng transform manipulation thay vì Animator (vì chưa có asset animator).
    /// Khi có Animator thật, có thể chuyển sang dùng Animator.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float walkBobSpeed = 8f;
        [SerializeField] private float walkBobAmount = 0.05f;
        [SerializeField] private float idleSwaySpeed = 1.5f;
        [SerializeField] private float idleSwayAmount = 0.02f;
        [SerializeField] private float pushBobSpeed = 6f;
        [SerializeField] private float pushBobAmount = 0.03f;

        [Header("References")]
        [SerializeField] private Transform visualTransform;

        private PlayerState currentState = PlayerState.Idle;
        private Vector3 originalPosition;
        private float animTimer;
        private Animator animator; // Optional — dùng nếu có

        private void Start()
        {
            if (visualTransform == null)
            {
                if (transform.childCount > 0)
                    visualTransform = transform.GetChild(0);
                else
                    visualTransform = transform;
            }

            originalPosition = visualTransform.localPosition;
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            animTimer += Time.deltaTime;

            // Nếu có Animator thật, dùng nó
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetInteger("State", (int)currentState);
                // Nếu animation 3D chưa đủ (vd Push, Serve), 
                // có thể kết hợp code procedural tại đây. Tạm thời return để test animation chuẩn.
                return;
            }

            // Animation thủ tục
            switch (currentState)
            {
                case PlayerState.Idle:
                    AnimateIdle();
                    break;
                case PlayerState.Walking:
                    AnimateWalk();
                    break;
                case PlayerState.PushingCart:
                    AnimatePushCart();
                    break;
                case PlayerState.Serving:
                    AnimateServing();
                    break;
                case PlayerState.Sitting:
                    AnimateSitting();
                    break;
            }
        }

        public void SetState(PlayerState state)
        {
            if (currentState == state) return;
            currentState = state;
            animTimer = 0f;
        }

        private void AnimateIdle()
        {
            // Đung đưa nhẹ khi đứng yên — tạo cảm giác sống động
            float sway = Mathf.Sin(animTimer * idleSwaySpeed) * idleSwayAmount;
            visualTransform.localPosition = originalPosition + new Vector3(0, sway, 0);
        }

        private void AnimateWalk()
        {
            // Nhún nhảy khi đi bộ
            float bob = Mathf.Abs(Mathf.Sin(animTimer * walkBobSpeed)) * walkBobAmount;
            visualTransform.localPosition = originalPosition + new Vector3(0, bob, 0);
        }

        private void AnimatePushCart()
        {
            // Nhún nhẹ hơn khi đẩy xe
            float bob = Mathf.Abs(Mathf.Sin(animTimer * pushBobSpeed)) * pushBobAmount;
            float lean = Mathf.Sin(animTimer * pushBobSpeed * 0.5f) * 2f;
            visualTransform.localPosition = originalPosition + new Vector3(0, bob, 0);
            visualTransform.localRotation = Quaternion.Euler(0, 0, lean);
        }

        private void AnimateServing()
        {
            // Animation phục vụ — nghiêng nhẹ về phía trước
            float lean = Mathf.Sin(animTimer * 3f) * 3f;
            visualTransform.localRotation = Quaternion.Euler(0, 0, -5f + lean);
        }

        private void AnimateSitting()
        {
            // Ngồi — hạ thấp nhẹ, thở nhẹ
            float breathe = Mathf.Sin(animTimer * 1f) * 0.01f;
            visualTransform.localPosition = originalPosition + new Vector3(0, -0.15f + breathe, 0);
            visualTransform.localRotation = Quaternion.identity;
        }
    }
}
