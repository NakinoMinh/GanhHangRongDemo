using UnityEngine;
using GanhHangRong.Core;

namespace GanhHangRong.Economy
{
    /// <summary>
    /// Quản lý thời gian trong ngày. Chuyển đổi các pha (Sáng, Trưa, Tối).
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float timeScaleMultiplier = Constants.GAME_MINUTES_PER_REAL_SECOND;
        
        private float currentHour = Constants.DAY_START_HOUR;
        private TimeOfDay currentTimeOfDay = TimeOfDay.EarlyMorning;

        public float CurrentHour => currentHour;
        public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying) return;

            // Chuyển đổi giây thực sang giờ trong game
            // 1 giây thực = timeScaleMultiplier phút game
            float deltaHours = (Time.deltaTime * timeScaleMultiplier) / 60f;
            currentHour += deltaHours;

            if (currentHour >= 24f)
            {
                currentHour -= 24f;
                GameManager.Instance.AdvanceDay();
            }

            EventManager.TriggerHourChanged(currentHour);
            UpdateTimeOfDay();
        }

        private void UpdateTimeOfDay()
        {
            TimeOfDay newTime = currentTimeOfDay;

            if (currentHour >= 5f && currentHour < 7f) newTime = TimeOfDay.EarlyMorning;
            else if (currentHour >= 7f && currentHour < 11f) newTime = TimeOfDay.Morning;
            else if (currentHour >= 11f && currentHour < 17f) newTime = TimeOfDay.Afternoon;
            else if (currentHour >= 17f && currentHour < 20f) newTime = TimeOfDay.Evening;
            else if (currentHour >= 20f || currentHour < 1f) newTime = TimeOfDay.Night;
            else if (currentHour >= 1f && currentHour < 5f) newTime = TimeOfDay.LateNight;

            if (newTime != currentTimeOfDay)
            {
                currentTimeOfDay = newTime;
                EventManager.TriggerTimeOfDayChanged(currentTimeOfDay);
            }
        }
    }
}
