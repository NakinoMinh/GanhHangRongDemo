using UnityEngine;
using GanhHangRong.Core;

namespace GanhHangRong.Player
{
    /// <summary>
    /// Camera side-view mượt mà — theo dõi nhân vật với độ trễ cinematic.
    /// </summary>
    public class CinematicCamera : MonoBehaviour
    {
        [Header("Mục Tiêu")]
        [SerializeField] private Transform target;

        [Header("Theo Dõi")]
        [SerializeField] private float smoothTime = Constants.CAMERA_SMOOTH_TIME;
        [SerializeField] private float yOffset = Constants.CAMERA_Y_OFFSET;
        [SerializeField] private float zOffset = Constants.CAMERA_Z_OFFSET;

        [Header("Look Ahead")]
        [SerializeField] private float lookAheadDistance = Constants.CAMERA_LOOK_AHEAD;
        [SerializeField] private float lookAheadSmooth = 0.5f;

        [Header("Dead Zone")]
        [SerializeField] private float deadZoneWidth = Constants.CAMERA_DEAD_ZONE;

        [Header("Giới Hạn")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private float minX = -20f;
        [SerializeField] private float maxX = 20f;

        private Vector3 velocity = Vector3.zero;
        private float currentLookAhead = 0f;
        private float lookAheadVelocity = 0f;

        private void Awake()
        {
            // Lấy zOffset từ vị trí ban đầu của Camera thay vì ghi đè bằng hằng số
            zOffset = transform.position.z;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Tính vị trí mục tiêu
            float targetX = target.position.x;

            // Look-ahead dựa trên hướng di chuyển
            var controller = target.GetComponent<PlayerController2D>();
            float targetLookAhead = 0f;
            if (controller != null && Mathf.Abs(controller.HorizontalInput) > 0.1f)
            {
                targetLookAhead = controller.FacingRight ? lookAheadDistance : -lookAheadDistance;
            }
            currentLookAhead = Mathf.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmooth);

            // Dead zone — chỉ di chuyển camera khi nhân vật ra ngoài dead zone
            float diff = targetX + currentLookAhead - transform.position.x;
            float targetPosX;
            if (Mathf.Abs(diff) < deadZoneWidth)
            {
                targetPosX = transform.position.x;
            }
            else
            {
                targetPosX = targetX + currentLookAhead;
            }

            // Vị trí mục tiêu cuối cùng
            Vector3 targetPos = new Vector3(
                targetPosX,
                target.position.y + yOffset,
                zOffset
            );

            // Giới hạn camera
            if (useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            }

            // Smooth follow
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(float min, float max)
        {
            minX = min;
            maxX = max;
        }

        /// <summary>
        /// Snap camera đến vị trí mục tiêu ngay lập tức (dùng khi load scene).
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = new Vector3(
                target.position.x,
                target.position.y + yOffset,
                zOffset
            );
            velocity = Vector3.zero;
        }
    }
}
