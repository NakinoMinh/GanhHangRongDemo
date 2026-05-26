using UnityEngine;
using GanhHangRong.Core;
using UnityEngine.InputSystem;

namespace GanhHangRong.Player
{
    /// <summary>
    /// Điều khiển nhân vật chính — Nguyễn Hoàng Hôn.
    /// Di chuyển ngang, tương tác, và quản lý trạng thái.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Di Chuyển")]
        [SerializeField] private float walkSpeed = Constants.PLAYER_WALK_SPEED;
        [SerializeField] private float runSpeed = Constants.PLAYER_RUN_SPEED;
        [SerializeField] private float pushCartSpeed = Constants.PLAYER_PUSH_CART_SPEED;

        [Header("Tương Tác")]
        [SerializeField] private float interactionRange = 1.5f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        // Components
        private Rigidbody2D rb;
        private PlayerAnimator playerAnimator;
        private PlayerStats playerStats;
        private SpriteRenderer spriteRenderer;

        // State
        private PlayerState currentState = PlayerState.Idle;
        private float horizontalInput;
        private bool facingRight = true;
        private bool canMove = true;
        private bool isPushingCart = false;
        private bool isRunning = false;
        private Interaction.Interactable nearestInteractable;

        public PlayerState CurrentState => currentState;
        public bool FacingRight => facingRight;
        public float HorizontalInput => horizontalInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerAnimator = GetComponent<PlayerAnimator>();
            playerStats = GetComponent<PlayerStats>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Cấu hình Rigidbody2D
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void OnEnable()
        {
            EventManager.OnGamePhaseChanged += HandleGamePhaseChanged;
            EventManager.OnDialogueStarted += DisableMovement;
            EventManager.OnDialogueEnded += EnableMovement;
        }

        private void OnDisable()
        {
            EventManager.OnGamePhaseChanged -= HandleGamePhaseChanged;
            EventManager.OnDialogueStarted -= DisableMovement;
            EventManager.OnDialogueEnded -= EnableMovement;
        }

        private void Update()
        {
            if (!canMove || !GameManager.Instance.IsPlaying) return;

            // Input
            horizontalInput = 0f;
            isRunning = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
                
                isRunning = Keyboard.current.shiftKey.isPressed;
            }

            // Tương tác
            CheckInteraction();
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }

            // Cập nhật trạng thái
            UpdatePlayerState();

            // Flip sprite
            if (horizontalInput > 0 && !facingRight) Flip();
            else if (horizontalInput < 0 && facingRight) Flip();
        }

        private void FixedUpdate()
        {
            if (!canMove || !GameManager.Instance.IsPlaying) return;

            // Di chuyển
            float speed = isPushingCart ? pushCartSpeed : (isRunning ? runSpeed : walkSpeed);

            // Áp dụng penalty mệt mỏi
            if (playerStats != null)
            {
                float fatiguePenalty = Mathf.Lerp(1f, 1f - Constants.PLAYER_SPEED_FATIGUE_PENALTY,
                    playerStats.Fatigue / Constants.PLAYER_FATIGUE_MAX);
                speed *= fatiguePenalty;
            }

            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }

        private void UpdatePlayerState()
        {
            PlayerState newState;

            if (isPushingCart && Mathf.Abs(horizontalInput) > 0.1f)
                newState = PlayerState.PushingCart;
            else if (Mathf.Abs(horizontalInput) > 0.1f)
                newState = isRunning ? PlayerState.Running : PlayerState.Walking;
            else
                newState = PlayerState.Idle;

            if (newState != currentState)
            {
                currentState = newState;
                EventManager.TriggerPlayerStateChanged(currentState);
                if (playerAnimator != null)
                    playerAnimator.SetState(currentState);
            }
        }

        private void CheckInteraction()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

            Interaction.Interactable closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<Interaction.Interactable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            if (closest != nearestInteractable)
            {
                nearestInteractable = closest;
                if (nearestInteractable != null)
                    EventManager.TriggerInteractionPromptShow(nearestInteractable.PromptText);
                else
                    EventManager.TriggerInteractionPromptHide();
            }
        }

        private void TryInteract()
        {
            if (nearestInteractable != null && nearestInteractable.CanInteract)
            {
                nearestInteractable.Interact(this);
            }
        }

        private void Flip()
        {
            facingRight = !facingRight;
            transform.Rotate(0f, 180f, 0f);
        }

        public void SetPushingCart(bool pushing)
        {
            isPushingCart = pushing;
        }

        public void SetState(PlayerState state)
        {
            currentState = state;
            EventManager.TriggerPlayerStateChanged(state);
            if (playerAnimator != null)
                playerAnimator.SetState(state);
        }

        public void EnableMovement() => canMove = true;
        public void DisableMovement() => canMove = false;

        private void HandleGamePhaseChanged(GamePhase phase)
        {
            canMove = phase == GamePhase.Playing;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
