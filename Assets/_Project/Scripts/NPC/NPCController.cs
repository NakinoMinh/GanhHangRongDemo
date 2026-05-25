using UnityEngine;
using GanhHangRong.Core;
using GanhHangRong.Interaction;
using System.Collections;

namespace GanhHangRong.NPC
{
    /// <summary>
    /// AI điều khiển một NPC khách hàng.
    /// Dùng State Machine đơn giản để mô phỏng hành vi.
    /// </summary>
    [RequireComponent(typeof(NPCAppearance))]
    public class NPCController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float stopDistance = 0.1f;

        private NPCProfile profile;
        private NPCState currentState = NPCState.Spawning;
        private CustomerSeat targetSeat;
        private Transform exitPoint;
        
        private float waitTimer = 0f;
        private float maxWaitTime;
        private float drinkTimer = 0f;
        private float drinkDuration;
        private bool isServed = false;
        
        private NPCAppearance appearance;

        public NPCState CurrentState => currentState;

        private void Awake()
        {
            appearance = GetComponent<NPCAppearance>();
        }

        public void Initialize(NPCProfile profile, CustomerSeat seat, Transform exit, float walkSpd)
        {
            this.profile = profile;
            this.targetSeat = seat;
            this.exitPoint = exit;
            this.walkSpeed = walkSpd;
            
            this.maxWaitTime = Random.Range(profile.minPatience, profile.maxPatience);
            this.drinkDuration = Random.Range(profile.minDrinkTime, profile.maxDrinkTime);
            
            if (appearance != null)
                appearance.ApplyProfile(profile);
                
            ChangeState(NPCState.WalkingIn);
        }

        private void Update()
        {
            if (GameManager.Instance.IsPaused) return;

            switch (currentState)
            {
                case NPCState.WalkingIn:
                    MoveTowards(targetSeat.transform.position, NPCState.SittingDown);
                    break;
                    
                case NPCState.SittingDown:
                    // Animation ngồi, sau đó chuyển sang Order
                    transform.position = targetSeat.transform.position; // Snap vào ghế
                    ChangeState(NPCState.Ordering);
                    break;
                    
                case NPCState.Ordering:
                    // Hiện bong bóng chat (tạm thời bỏ qua, chuyển luôn sang chờ)
                    EventManager.TriggerCustomerArrived(profile.npcType);
                    ChangeState(NPCState.Waiting);
                    break;
                    
                case NPCState.Waiting:
                    waitTimer += Time.deltaTime;
                    if (isServed)
                    {
                        ChangeState(NPCState.Drinking);
                        EventManager.TriggerCustomerServed(profile.npcType);
                    }
                    else if (waitTimer >= maxWaitTime)
                    {
                        ChangeState(NPCState.LeavingSad);
                        EventManager.TriggerCustomerLeftSad(profile.npcType);
                        if (targetSeat != null) targetSeat.FreeSeat();
                    }
                    break;
                    
                case NPCState.Drinking:
                    drinkTimer += Time.deltaTime;
                    if (drinkTimer >= drinkDuration)
                    {
                        ChangeState(NPCState.Paying);
                    }
                    break;
                    
                case NPCState.Paying:
                    PayForDrink();
                    ChangeState(NPCState.LeavingHappy);
                    EventManager.TriggerCustomerLeftHappy(profile.npcType);
                    if (targetSeat != null) targetSeat.FreeSeat();
                    break;
                    
                case NPCState.LeavingHappy:
                case NPCState.LeavingSad:
                    ChangeState(NPCState.WalkingOut);
                    break;
                    
                case NPCState.WalkingOut:
                    MoveTowards(exitPoint.position, NPCState.Spawning /* Destroy */);
                    if (Vector2.Distance(transform.position, exitPoint.position) <= stopDistance)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }

        private void MoveTowards(Vector3 target, NPCState nextState)
        {
            float step = walkSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, transform.position.z), step);

            // Flip mặt
            if (target.x > transform.position.x)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (target.x < transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            if (Mathf.Abs(transform.position.x - target.x) <= stopDistance)
            {
                if (nextState != NPCState.Spawning) // Mượn Spawning làm cờ Destroy
                    ChangeState(nextState);
            }
        }

        private void ChangeState(NPCState newState)
        {
            currentState = newState;
        }

        public void ServeDrink()
        {
            if (currentState == NPCState.Waiting && !isServed)
            {
                isServed = true;
            }
        }

        private void PayForDrink()
        {
            int basePrice = Constants.TRA_DA_SELL_PRICE;
            int total = basePrice;
            
            // Tính tip
            if (Random.value <= profile.tipChance)
            {
                total += 2000; // Tip 2k
            }
            
            // Tìm PlayerStats để cộng tiền
            var playerStats = FindAnyObjectByType<Player.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddMoney(total);
                playerStats.RecordCustomerServed();
            }
        }
    }
}
