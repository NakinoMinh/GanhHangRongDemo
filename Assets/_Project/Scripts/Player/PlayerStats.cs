using UnityEngine;
using GanhHangRong.Core;

namespace GanhHangRong.Player
{
    /// <summary>
    /// Thống kê nhân vật — tiền, mệt mỏi, mức đá.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Thống Kê")]
        [SerializeField] private int money = Constants.STARTING_MONEY;
        [SerializeField] private float fatigue = 0f;
        [SerializeField] private float iceLevel = Constants.ICE_MAX;

        [Header("Kho Đồ")]
        [SerializeField] private int teaSupply = 10;
        [SerializeField] private int sugarSupply = 10;
        [SerializeField] private int cupSupply = 20;

        // Properties
        public int Money => money;
        public float Fatigue => fatigue;
        public float IceLevel => iceLevel;
        public int TeaSupply => teaSupply;
        public int SugarSupply => sugarSupply;
        public int CupSupply => cupSupply;

        // Tracking cho hệ thống cảm xúc
        private int customersServedToday = 0;
        private int moneyEarnedToday = 0;
        public int CustomersServedToday => customersServedToday;
        public int MoneyEarnedToday => moneyEarnedToday;

        private void OnEnable()
        {
            EventManager.OnNewDay += ResetDailyStats;
        }

        private void OnDisable()
        {
            EventManager.OnNewDay -= ResetDailyStats;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying) return;

            // Mệt mỏi tăng dần
            var controller = GetComponent<PlayerController2D>();
            if (controller != null && controller.CurrentState == PlayerState.Sitting)
            {
                ModifyFatigue(-Constants.PLAYER_FATIGUE_REST_RATE * Time.deltaTime / 60f);
            }
            else
            {
                ModifyFatigue(Constants.PLAYER_FATIGUE_RATE * Time.deltaTime / 60f);
            }

            // Đá tan dần
            float meltRate = Constants.ICE_MELT_RATE;
            // Mưa làm đá tan chậm hơn (lạnh hơn)
            // WeatherManager sẽ gọi SetIceMeltModifier nếu cần
            ModifyIceLevel(-meltRate * Time.deltaTime / 60f);
        }

        // ═══════════════════════════════════════════
        // TIỀN
        // ═══════════════════════════════════════════
        public bool CanAfford(int amount) => money >= amount;

        public void AddMoney(int amount)
        {
            money += amount;
            moneyEarnedToday += amount;
            EventManager.TriggerMoneyChanged(money);
            EventManager.TriggerMoneyEarned(amount);
        }

        public bool SpendMoney(int amount)
        {
            if (!CanAfford(amount)) return false;
            money -= amount;
            EventManager.TriggerMoneyChanged(money);
            EventManager.TriggerMoneySpent(amount);
            return true;
        }

        // ═══════════════════════════════════════════
        // MỆT MỎI
        // ═══════════════════════════════════════════
        public void ModifyFatigue(float amount)
        {
            float prev = fatigue;
            fatigue = Mathf.Clamp(fatigue + amount, 0f, Constants.PLAYER_FATIGUE_MAX);
            if (!Mathf.Approximately(prev, fatigue))
                EventManager.TriggerFatigueChanged(fatigue);
        }

        // ═══════════════════════════════════════════
        // ĐÁ
        // ═══════════════════════════════════════════
        public void ModifyIceLevel(float amount)
        {
            float prev = iceLevel;
            iceLevel = Mathf.Clamp(iceLevel + amount, 0f, Constants.ICE_MAX);
            if (!Mathf.Approximately(prev, iceLevel))
                EventManager.TriggerIceLevelChanged(iceLevel);
        }

        public void RefillIce()
        {
            iceLevel = Constants.ICE_MAX;
            EventManager.TriggerIceLevelChanged(iceLevel);
        }

        // ═══════════════════════════════════════════
        // KHO ĐỒ
        // ═══════════════════════════════════════════
        public bool HasSuppliesForTea()
        {
            return teaSupply > 0 && sugarSupply > 0 && cupSupply > 0 && iceLevel > 5f;
        }

        public void UseTeaSupplies()
        {
            teaSupply--;
            sugarSupply--;
            cupSupply--;
        }

        public void AddSupplies(int tea, int sugar, int cups)
        {
            teaSupply += tea;
            sugarSupply += sugar;
            cupSupply += cups;
        }

        public void RecordCustomerServed()
        {
            customersServedToday++;
        }

        private void ResetDailyStats()
        {
            customersServedToday = 0;
            moneyEarnedToday = 0;
        }
    }
}
