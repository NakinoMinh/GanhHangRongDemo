using UnityEngine;
using GanhHangRong.Core;
using GanhHangRong.Interaction;
using GanhHangRong.Weather;
using System.Collections.Generic;

namespace GanhHangRong.NPC
{
    /// <summary>
    /// Quản lý việc sinh ra khách hàng dựa trên thời gian, thời tiết và trạng thái cảm xúc.
    /// </summary>
    public class NPCSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform[] exitPoints;
        [SerializeField] private List<NPCProfile> availableProfiles;
        
        private float spawnTimer = 0f;
        private int currentCustomerCount = 0;

        private void OnEnable()
        {
            EventManager.OnCustomerArrived += OnCustomerAdded;
            EventManager.OnCustomerLeftHappy += OnCustomerRemoved;
            EventManager.OnCustomerLeftSad += OnCustomerRemoved;
        }

        private void OnDisable()
        {
            EventManager.OnCustomerArrived -= OnCustomerAdded;
            EventManager.OnCustomerLeftHappy -= OnCustomerRemoved;
            EventManager.OnCustomerLeftSad -= OnCustomerRemoved;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying || 
                GameManager.Instance.CurrentPhase != GamePhase.Playing) 
                return;

            // Chỉ spawn khách vào ban đêm
            var timeManager = FindAnyObjectByType<Economy.DayNightCycle>();
            if (timeManager != null && timeManager.CurrentTimeOfDay != TimeOfDay.Night)
                return;

            if (currentCustomerCount >= Constants.MAX_CONCURRENT_CUSTOMERS) return;

            spawnTimer += Time.deltaTime;

            float currentSpawnInterval = CalculateSpawnInterval();

            if (spawnTimer >= currentSpawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnNPC();
            }
        }

        private float CalculateSpawnInterval()
        {
            float interval = Constants.NPC_SPAWN_INTERVAL_BASE;
            
            // Thời tiết ảnh hưởng
            if (WeatherManager.HasInstance && WeatherManager.Instance.CurrentPreset != null)
            {
                float modifier = WeatherManager.Instance.CurrentPreset.customerSpawnModifier;
                if (modifier > 0) interval /= modifier; // Modifier nhỏ -> Interval lớn -> Ít khách
            }

            // Emotional level ảnh hưởng (càng buồn/tuyệt vọng càng ít khách)
            switch (GameManager.Instance.CurrentEmotionalLevel)
            {
                case EmotionalLevel.Hopeful: interval *= 0.8f; break; // Spawn nhanh hơn
                case EmotionalLevel.Normal: break;
                case EmotionalLevel.Struggling: interval *= 1.5f; break;
                case EmotionalLevel.Lonely: interval *= 2.5f; break;
                case EmotionalLevel.Desperate: interval *= 4f; break;
            }

            return interval;
        }

        private void TrySpawnNPC()
        {
            // Tìm ghế trống
            var allSeats = FindObjectsByType<CustomerSeat>(FindObjectsInactive.Exclude);
            CustomerSeat emptySeat = null;
            
            // Shuffle nhẹ để lấy random ghế (tối ưu hóa sau)
            List<CustomerSeat> seatList = new List<CustomerSeat>(allSeats);
            for (int i = 0; i < seatList.Count; i++)
            {
                int r = Random.Range(i, seatList.Count);
                var temp = seatList[i];
                seatList[i] = seatList[r];
                seatList[r] = temp;
            }

            foreach (var seat in seatList)
            {
                if (!seat.IsOccupied)
                {
                    emptySeat = seat;
                    break;
                }
            }

            if (emptySeat == null) return; // Hết chỗ

            // Spawn
            emptySeat.OccupySeat(); // Book chỗ trước

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Transform exitPoint = exitPoints[Random.Range(0, exitPoints.Length)];
            
            NPCProfile profile = availableProfiles[Random.Range(0, availableProfiles.Count)];
            float speed = Random.Range(Constants.NPC_WALK_SPEED_MIN, Constants.NPC_WALK_SPEED_MAX);

            GameObject npcObj = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
            NPCController controller = npcObj.GetComponent<NPCController>();
            
            if (controller != null)
            {
                controller.Initialize(profile, emptySeat, exitPoint, speed);
                currentCustomerCount++;
            }
            else
            {
                emptySeat.FreeSeat();
                Destroy(npcObj);
            }
        }

        private void OnCustomerAdded(NPCType type) { }
        
        private void OnCustomerRemoved(NPCType type)
        {
            currentCustomerCount--;
            if (currentCustomerCount < 0) currentCustomerCount = 0;
        }
    }
}
