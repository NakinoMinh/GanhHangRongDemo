using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using GanhHangRong.Core;
using GanhHangRong.Player;
using GanhHangRong.NPC;
using GanhHangRong.Interaction;
using GanhHangRong.Weather;
using GanhHangRong.Audio;
using GanhHangRong.Economy;
using GanhHangRong.UI;
using System.Collections.Generic;

namespace GanhHangRong.Editor
{
    public static class Chapter1SceneBuilder
    {
        [MenuItem("Gánh Hàng Rong/Dựng Scene Chapter 1", false, 12)]
        public static void BuildChapter1Scene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "Chapter1";

            // TẠO VẬT LIỆU TẠM
            Material matToon = CreateMaterial("ToonMat", "GanhHangRong/ToonShading", Color.white);
            Material matPlayer = CreateMaterial("PlayerMat", "GanhHangRong/ToonShading", new Color(0.9f, 0.4f, 0.2f));
            Material matRoad = CreateMaterial("RoadMat", "GanhHangRong/WetGround", new Color(0.15f, 0.15f, 0.18f));
            Material matWater = CreateMaterial("WaterMat", "GanhHangRong/WaterSurface", new Color(0f, 0.3f, 0.5f));
            Material matCart = CreateMaterial("CartMat", "GanhHangRong/ToonShading", new Color(0.6f, 0.4f, 0.2f));
            Material matChair = CreateMaterial("ChairMat", "GanhHangRong/ToonShading", new Color(0.8f, 0.1f, 0.1f));

            // ==========================================
            // 1. ÁNH SÁNG
            // ==========================================
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.8f, 0.8f, 1f);
            dirLight.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // ==========================================
            // 2. CAMERA & POST PROCESSING (Giả lập)
            // ==========================================
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.orthographic = false;
            cam.fieldOfView = 30f; // INSIDE style
            camObj.transform.position = new Vector3(0, 2, -15);
            var cinCam = camObj.AddComponent<CinematicCamera>();

            // ==========================================
            // 3. MÔI TRƯỜNG & HỆ THỐNG PARALLAX
            // ==========================================
            GameObject envParent = new GameObject("Environment");
            var parallaxSys = envParent.AddComponent<GanhHangRong.Systems.ParallaxSystem>();

            // Lớp 1: Mặt biển (Parallax 0.1)
            GameObject seaObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            seaObj.name = "Sea_Layer1";
            seaObj.transform.SetParent(envParent.transform);
            seaObj.transform.position = new Vector3(0, 0, 10);
            seaObj.transform.localScale = new Vector3(100, 20, 1);
            seaObj.GetComponent<MeshRenderer>().sharedMaterial = matWater;
            var pl1 = seaObj.AddComponent<GanhHangRong.Systems.ParallaxLayer>();
            pl1.GetType().GetField("speedMultiplierX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pl1, 0.1f);

            // Lớp 2: Mặt đường bến tàu (Ground)
            GameObject roadObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadObj.name = "Road_Ground";
            roadObj.transform.SetParent(envParent.transform);
            roadObj.transform.position = new Vector3(0, -1f, 0);
            roadObj.transform.localScale = new Vector3(60, 2, 5);
            roadObj.GetComponent<MeshRenderer>().sharedMaterial = matRoad;
            Object.DestroyImmediate(roadObj.GetComponent<BoxCollider>());
            roadObj.AddComponent<BoxCollider2D>();
            roadObj.layer = LayerMask.NameToLayer("Default"); // Tạm để collider mặc định

            // ==========================================
            // 4. XE TRÀ ĐÁ & GHẾ NHỰA
            // ==========================================
            GameObject cartObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartObj.name = "TeaCart";
            cartObj.transform.position = new Vector3(2, 1, 0);
            cartObj.transform.localScale = new Vector3(2, 2, 1);
            cartObj.GetComponent<MeshRenderer>().sharedMaterial = matCart;
            Object.DestroyImmediate(cartObj.GetComponent<BoxCollider>());
            var cartCollider = cartObj.AddComponent<BoxCollider2D>();
            cartCollider.isTrigger = true; // Cho phép đi xuyên tương tác
            var cartScript = cartObj.AddComponent<TeaCart>();
            
            // Đèn dầu trên xe
            GameObject lanternObj = new GameObject("LanternLight");
            lanternObj.transform.SetParent(cartObj.transform);
            lanternObj.transform.localPosition = new Vector3(0, 0.6f, 0);
            Light lanternLight = lanternObj.AddComponent<Light>();
            lanternLight.type = LightType.Point;
            lanternLight.color = new Color(1f, 0.7f, 0.3f);
            lanternLight.intensity = 1.5f;
            lanternLight.range = 5f;

            // Ghế nhựa
            for (int i = 0; i < 4; i++)
            {
                GameObject chairObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chairObj.name = $"Chair_{i}";
                chairObj.transform.position = new Vector3(-2 + (i * 1.5f), 0.25f, 0);
                chairObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                chairObj.GetComponent<MeshRenderer>().sharedMaterial = matChair;
                Object.DestroyImmediate(chairObj.GetComponent<BoxCollider>());
                var chairCollider = chairObj.AddComponent<BoxCollider2D>();
                chairCollider.isTrigger = true;
                chairObj.AddComponent<CustomerSeat>();
            }

            GameObject playerObj = new GameObject("Player_HoangHon");
            playerObj.tag = "Player";
            playerObj.layer = LayerMask.NameToLayer("Default");
            playerObj.transform.position = new Vector3(0, 1.5f, 0);
            
            // Trả lại đường dẫn model cũ của User
            string modelPath = "Assets/_Project/Art/Models/Player/Meshy_AI_Ripped_Jeans_Portrait_biped/Meshy_AI_Ripped_Jeans_Portrait_biped_Animation_Walking_withSkin.glb";
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            
            if (modelPrefab != null)
            {
                GameObject visualObj = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
                visualObj.transform.SetParent(playerObj.transform);
                // Vì mô hình GLB thường có gốc tọa độ ở tâm (hoặc khác chuẩn), ta đặt localPosition = 0
                // để tránh tình trạng nhân vật bị lún xuống đất một nửa.
                visualObj.transform.localPosition = Vector3.zero; 
                visualObj.transform.localRotation = Quaternion.Euler(0, 90, 0); // Xoay ngang
                
                SetupPlayerAnimator(visualObj);
            }
            else
            {
                // Fallback nếu không có model
                GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visualObj.transform.SetParent(playerObj.transform);
                visualObj.transform.localPosition = new Vector3(0, 1f, 0);
                visualObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                Object.DestroyImmediate(visualObj.GetComponent<CapsuleCollider>());
                visualObj.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial("Mat_PlayerFallback", "Standard", Color.blue);
            }
            
            var col2d = playerObj.AddComponent<CapsuleCollider2D>();
            col2d.size = new Vector2(0.5f, 1.8f);
            
            playerObj.AddComponent<Rigidbody2D>().freezeRotation = true;
            playerObj.AddComponent<PlayerController2D>();
            playerObj.AddComponent<PlayerAnimator>();
            playerObj.AddComponent<PlayerStats>();
            cinCam.SetTarget(playerObj.transform); // Set camera target

            // ==========================================
            // 5. HIỆU ỨNG THỜI TIẾT (PARTICLES)
            // ==========================================
            GameObject weatherFxObj = new GameObject("WeatherFX");
            
            // Mưa
            GameObject rainObj = new GameObject("RainParticles");
            rainObj.transform.SetParent(weatherFxObj.transform);
            rainObj.transform.position = new Vector3(0, 8, 0); // Đặt trên cao
            var rainPs = rainObj.AddComponent<ParticleSystem>();
            var rainMain = rainPs.main;
            rainMain.loop = true;
            rainMain.startLifetime = 1.5f;
            rainMain.startSpeed = 15f;
            rainMain.startSize = 0.05f;
            rainMain.maxParticles = 2000;
            rainMain.simulationSpace = ParticleSystemSimulationSpace.World;
            rainMain.startColor = new Color(0.8f, 0.85f, 1f, 0.4f);
            rainMain.gravityModifier = 0.5f;
            
            var rainEmission = rainPs.emission;
            rainEmission.rateOverTime = 0; // Quản lý bởi WeatherManager
            
            var rainShape = rainPs.shape;
            rainShape.shapeType = ParticleSystemShapeType.Box;
            rainShape.scale = new Vector3(30, 0, 10);
            
            var rainRenderer = rainObj.GetComponent<ParticleSystemRenderer>();
            rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            rainRenderer.lengthScale = 8f;
            rainRenderer.velocityScale = 0.1f;
            rainRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");

            // Sương mù
            GameObject fogObj = new GameObject("FogParticles");
            fogObj.transform.SetParent(weatherFxObj.transform);
            fogObj.transform.position = new Vector3(0, -0.5f, 5); // Đặt thấp, xa một chút
            var fogPs = fogObj.AddComponent<ParticleSystem>();
            var fogMain = fogPs.main;
            fogMain.loop = true;
            fogMain.startLifetime = 8f;
            fogMain.startSpeed = 1f;
            fogMain.startSize = new ParticleSystem.MinMaxCurve(3f, 8f);
            fogMain.maxParticles = 200;
            fogMain.simulationSpace = ParticleSystemSimulationSpace.World;
            fogMain.startColor = new Color(1f, 1f, 1f, 0.1f);
            
            var fogEmission = fogPs.emission;
            fogEmission.rateOverTime = 0;
            
            var fogShape = fogPs.shape;
            fogShape.shapeType = ParticleSystemShapeType.Box;
            fogShape.scale = new Vector3(40, 1, 15);
            
            var fogRenderer = fogObj.GetComponent<ParticleSystemRenderer>();
            fogRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");

            // ==========================================
            // 6. CÁC HỆ THỐNG QUẢN LÝ (MANAGERS)
            // ==========================================
            GameObject managersObj = new GameObject("Managers");
            managersObj.AddComponent<GameManager>();
            
            // Weather
            var weatherMgr = managersObj.AddComponent<WeatherManager>();
            var rainSys = managersObj.AddComponent<RainSystem>();
            var windSys = managersObj.AddComponent<WindSystem>();
            var fogCtrl = managersObj.AddComponent<FogController>();
            
            // Gán Particle cho Weather systems
            var serializedRain = new SerializedObject(rainSys);
            serializedRain.FindProperty("rainParticles").objectReferenceValue = rainPs;
            serializedRain.ApplyModifiedPropertiesWithoutUndo();
            
            windSys.Initialize(rainPs);
            
            var serializedFog = new SerializedObject(fogCtrl);
            serializedFog.FindProperty("fogParticles").objectReferenceValue = fogPs;
            serializedFog.ApplyModifiedPropertiesWithoutUndo();

            // Tạo Weather Presets
            List<WeatherPreset> presets = new List<WeatherPreset>();
            presets.Add(CreateWeatherPreset("Clear", WeatherType.Clear, new Color(0.8f, 0.9f, 1f), 0.8f, 0, 0, 0.001f, 1f));
            presets.Add(CreateWeatherPreset("LightRain", WeatherType.LightRain, new Color(0.6f, 0.7f, 0.8f), 0.5f, 0.4f, 0.3f, 0.01f, 0.7f));
            presets.Add(CreateWeatherPreset("HeavyRain", WeatherType.HeavyRain, new Color(0.4f, 0.4f, 0.5f), 0.3f, 1f, 0.7f, 0.03f, 0.3f));
            presets.Add(CreateWeatherPreset("SeaWind", WeatherType.SeaWind, new Color(0.7f, 0.8f, 0.9f), 0.7f, 0.1f, 0.8f, 0.005f, 0.8f));
            presets.Add(CreateWeatherPreset("Foggy", WeatherType.Foggy, new Color(0.9f, 0.9f, 0.95f), 0.4f, 0, 0.1f, 0.04f, 0.6f));
            
            var serializedWeatherMgr = new SerializedObject(weatherMgr);
            var presetsProp = serializedWeatherMgr.FindProperty("weatherPresets");
            presetsProp.ClearArray();
            for (int i = 0; i < presets.Count; i++)
            {
                presetsProp.InsertArrayElementAtIndex(i);
                presetsProp.GetArrayElementAtIndex(i).objectReferenceValue = presets[i];
            }
            serializedWeatherMgr.ApplyModifiedPropertiesWithoutUndo();

            managersObj.AddComponent<EconomyManager>();
            managersObj.AddComponent<DayNightCycle>();
            managersObj.AddComponent<GanhHangRong.Systems.TimeOfDayLighting>();
            managersObj.AddComponent<Narrative.DialogueManager>();
            managersObj.AddComponent<AudioManager>();
            managersObj.AddComponent<GanhHangRong.Systems.EmotionalFailureSystem>();
            managersObj.AddComponent<GanhHangRong.Systems.GameplayLoop>();
            managersObj.AddComponent<NPCVisualFactory>();

            // Thiết lập field cho WeatherManager
            weatherMgr.GetType().GetField("globalLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(weatherMgr, dirLight);

            // ==========================================
            // 7. HỆ THỐNG NPC SPAWNER + PROFILES
            // ==========================================
            // Tạo 5 NPCProfile
            List<NPCProfile> profiles = new List<NPCProfile>();
            profiles.Add(CreateNPCProfile("Fisherman", NPCType.Fisherman, 20, 40, 5, 10, 0.3f));
            profiles.Add(CreateNPCProfile("Worker", NPCType.Worker, 15, 30, 3, 8, 0.2f));
            profiles.Add(CreateNPCProfile("BusDriver", NPCType.BusDriver, 10, 25, 4, 7, 0.1f));
            profiles.Add(CreateNPCProfile("IslandTraveler", NPCType.IslandTraveler, 25, 50, 8, 15, 0.5f));
            profiles.Add(CreateNPCProfile("LocalResident", NPCType.LocalResident, 30, 60, 5, 12, 0.4f));

            GameObject spawnerObj = new GameObject("NPC_Spawner");
            var spawner = spawnerObj.AddComponent<NPCSpawner>();
            
            GameObject spawnLeft = new GameObject("SpawnPoint_Left");
            spawnLeft.transform.position = new Vector3(-20, 1, 0);
            spawnLeft.transform.SetParent(spawnerObj.transform);
            
            GameObject spawnRight = new GameObject("SpawnPoint_Right");
            spawnRight.transform.position = new Vector3(20, 1, 0);
            spawnRight.transform.SetParent(spawnerObj.transform);
            
            var bf = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            spawner.GetType().GetField("spawnPoints", bf).SetValue(spawner, new Transform[] { spawnLeft.transform, spawnRight.transform });
            spawner.GetType().GetField("exitPoints", bf).SetValue(spawner, new Transform[] { spawnLeft.transform, spawnRight.transform });
            spawner.GetType().GetField("availableProfiles", bf).SetValue(spawner, profiles);

            // ==========================================
            // 8. HỆ THỐNG UI (REDESIGNED)
            // ==========================================
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            GameObject hudObj = new GameObject("HUD");
            hudObj.transform.SetParent(canvasObj.transform, false);
            var hudRect = hudObj.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero; hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero; hudRect.offsetMax = Vector2.zero;
            hudObj.AddComponent<CanvasGroup>();
            var gameplayHUD = hudObj.AddComponent<GameplayHUD>(); 
            
            // Tải Sprites (từ ảnh mockup đã crop)
            Sprite sprAvatarBoard = LoadSprite("Assets/_Project/Art/UI/Items/khung_avatar.png");
            Sprite sprPriceTag = LoadSprite("Assets/_Project/Art/UI/Items/the_gia.png");
            Sprite sprCompass = LoadSprite("Assets/_Project/Art/UI/Items/dong_ho.png");
            Sprite sprCounter = LoadSprite("Assets/_Project/Art/UI/Items/mat_ban_inox.png");
            Sprite sprTea = LoadSprite("Assets/_Project/Art/UI/Items/hu_tra.png");
            Sprite sprSugar = LoadSprite("Assets/_Project/Art/UI/Items/hu_duong.png");
            Sprite sprCup = LoadSprite("Assets/_Project/Art/UI/Items/ly_cups.png");
            Sprite sprIce = LoadSprite("Assets/_Project/Art/UI/Items/ice_box.png");

            // 1. Top Left - Avatar & Stats
            GameObject topLeftPanel = new GameObject("TopLeftPanel");
            topLeftPanel.transform.SetParent(hudObj.transform, false);
            var tlRect = topLeftPanel.AddComponent<RectTransform>();
            tlRect.anchorMin = new Vector2(0, 1); tlRect.anchorMax = new Vector2(0, 1);
            tlRect.pivot = new Vector2(0, 1);
            tlRect.anchoredPosition = new Vector2(50, -50);
            tlRect.sizeDelta = new Vector2(600, 200);
            
            if (sprAvatarBoard != null) {
                var tlImg = topLeftPanel.AddComponent<UnityEngine.UI.Image>();
                tlImg.sprite = sprAvatarBoard;
                tlImg.preserveAspect = true;
            }

            var nameText = new GameObject("NameText").AddComponent<TMPro.TextMeshProUGUI>();
            nameText.transform.SetParent(topLeftPanel.transform, false);
            nameText.rectTransform.anchoredPosition = new Vector2(40, 20);
            nameText.rectTransform.sizeDelta = new Vector2(300, 50);
            nameText.text = "NGUYỄN HOÀNG HÔN";
            nameText.fontSize = 28; nameText.alignment = TMPro.TextAlignmentOptions.Center;
            nameText.fontStyle = TMPro.FontStyles.Bold;
            nameText.color = new Color(0.9f, 0.8f, 0.7f); // Màu gỗ/kem

            // Năng lượng
            GameObject energyBg = new GameObject("EnergyBg");
            energyBg.transform.SetParent(topLeftPanel.transform, false);
            var ebgRect = energyBg.AddComponent<RectTransform>();
            ebgRect.anchoredPosition = new Vector2(40, -30); ebgRect.sizeDelta = new Vector2(250, 30);
            energyBg.AddComponent<UnityEngine.UI.Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            GameObject energyFill = new GameObject("EnergyFill");
            energyFill.transform.SetParent(energyBg.transform, false);
            var efillRect = energyFill.AddComponent<RectTransform>();
            efillRect.anchorMin = new Vector2(0, 0); efillRect.anchorMax = new Vector2(1, 1);
            efillRect.offsetMin = Vector2.zero; efillRect.offsetMax = Vector2.zero;
            var energySliderImg = energyFill.AddComponent<UnityEngine.UI.Image>();
            energySliderImg.color = new Color(1f, 0.6f, 0f); // Cam vàng

            var energySlider = energyBg.AddComponent<UnityEngine.UI.Slider>();
            energySlider.fillRect = efillRect; energySlider.minValue = 0; energySlider.maxValue = 1; energySlider.value = 1;
            energySlider.interactable = false;

            // Stress
            GameObject stressBg = new GameObject("StressBg");
            stressBg.transform.SetParent(topLeftPanel.transform, false);
            var sbgRect = stressBg.AddComponent<RectTransform>();
            sbgRect.anchoredPosition = new Vector2(40, -65); sbgRect.sizeDelta = new Vector2(250, 10);
            stressBg.AddComponent<UnityEngine.UI.Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            GameObject stressFill = new GameObject("StressFill");
            stressFill.transform.SetParent(stressBg.transform, false);
            var sfillRect = stressFill.AddComponent<RectTransform>();
            sfillRect.anchorMin = new Vector2(0, 0); sfillRect.anchorMax = new Vector2(1, 1);
            sfillRect.offsetMin = Vector2.zero; sfillRect.offsetMax = Vector2.zero;
            var stressSliderImg = stressFill.AddComponent<UnityEngine.UI.Image>();
            stressSliderImg.color = new Color(0.8f, 0.1f, 0.1f); // Đỏ

            var stressSlider = stressBg.AddComponent<UnityEngine.UI.Slider>();
            stressSlider.fillRect = sfillRect; stressSlider.minValue = 0; stressSlider.maxValue = 1; stressSlider.value = 0;
            stressSlider.interactable = false;

            // Avatar viền tròn
            GameObject avatarBox = new GameObject("AvatarBox");
            avatarBox.transform.SetParent(topLeftPanel.transform, false);
            var avBoxRect = avatarBox.AddComponent<RectTransform>();
            avBoxRect.anchoredPosition = new Vector2(-150, 0); avBoxRect.sizeDelta = new Vector2(120, 120);
            avatarBox.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);

            // 2. Top Right - Tiền
            GameObject topRightPanel = new GameObject("MoneyPanel");
            topRightPanel.transform.SetParent(hudObj.transform, false);
            var trRect = topRightPanel.AddComponent<RectTransform>();
            trRect.anchorMin = new Vector2(1, 1); trRect.anchorMax = new Vector2(1, 1);
            trRect.pivot = new Vector2(1, 1);
            trRect.anchoredPosition = new Vector2(-250, -50);
            trRect.sizeDelta = new Vector2(400, 120);
            
            if (sprPriceTag != null) {
                var trImg = topRightPanel.AddComponent<UnityEngine.UI.Image>();
                trImg.sprite = sprPriceTag;
                trImg.preserveAspect = true;
            }

            var moneyText = new GameObject("MoneyText").AddComponent<TMPro.TextMeshProUGUI>();
            moneyText.transform.SetParent(topRightPanel.transform, false);
            moneyText.rectTransform.anchoredPosition = new Vector2(30, 0);
            moneyText.rectTransform.sizeDelta = new Vector2(280, 80);
            moneyText.text = "50,000 VNĐ";
            moneyText.fontSize = 42; moneyText.alignment = TMPro.TextAlignmentOptions.Center;
            moneyText.color = new Color(0.3f, 0.2f, 0.1f);
            moneyText.fontStyle = TMPro.FontStyles.Bold;

            // 3. Top Right - Đồng hồ La Bàn
            GameObject clockPanel = new GameObject("ClockPanel");
            clockPanel.transform.SetParent(hudObj.transform, false);
            var clRect = clockPanel.AddComponent<RectTransform>();
            clRect.anchorMin = new Vector2(1, 1); clRect.anchorMax = new Vector2(1, 1);
            clRect.pivot = new Vector2(1, 1);
            clRect.anchoredPosition = new Vector2(-50, -50);
            clRect.sizeDelta = new Vector2(150, 150);
            
            if (sprCompass != null) {
                var clImg = clockPanel.AddComponent<UnityEngine.UI.Image>();
                clImg.sprite = sprCompass;
                clImg.preserveAspect = true;
            }

            var clockText = new GameObject("ClockText").AddComponent<TMPro.TextMeshProUGUI>();
            clockText.transform.SetParent(clockPanel.transform, false);
            clockText.rectTransform.anchoredPosition = new Vector2(0, 0);
            clockText.rectTransform.sizeDelta = new Vector2(150, 40);
            clockText.text = "17:00";
            clockText.fontSize = 28; clockText.alignment = TMPro.TextAlignmentOptions.Center;
            clockText.color = Color.white;
            clockText.fontStyle = TMPro.FontStyles.Bold;
            var clockOutline = clockText.gameObject.AddComponent<UnityEngine.UI.Outline>();
            clockOutline.effectColor = Color.black; clockOutline.effectDistance = new Vector2(2, -2);

            // 4. Bottom Center - Mặt Bàn Pha Chế
            GameObject invPanel = new GameObject("InventoryPanel");
            invPanel.transform.SetParent(hudObj.transform, false);
            var invRect = invPanel.AddComponent<RectTransform>();
            invRect.anchorMin = new Vector2(0.5f, 0); invRect.anchorMax = new Vector2(0.5f, 0);
            invRect.pivot = new Vector2(0.5f, 0);
            invRect.anchoredPosition = new Vector2(0, 0);
            invRect.sizeDelta = new Vector2(1200, 200);
            
            if (sprCounter != null) {
                var invImg = invPanel.AddComponent<UnityEngine.UI.Image>();
                invImg.sprite = sprCounter;
                invImg.type = UnityEngine.UI.Image.Type.Simple;
                invImg.preserveAspect = false; // Stretch để fill toàn bộ thanh dưới
            } else {
                invPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }
            
            var invTitle = new GameObject("Title").AddComponent<TMPro.TextMeshProUGUI>();
            invTitle.transform.SetParent(invPanel.transform, false);
            invTitle.rectTransform.anchoredPosition = new Vector2(-400, 80);
            invTitle.rectTransform.sizeDelta = new Vector2(300, 40);
            invTitle.text = "Mặt Bàn Pha Chế";
            invTitle.fontSize = 24; invTitle.alignment = TMPro.TextAlignmentOptions.Center;
            invTitle.color = new Color(1f, 0.9f, 0.8f);
            invTitle.fontStyle = TMPro.FontStyles.Bold;

            var hlgObj = new GameObject("SlotContainer");
            hlgObj.transform.SetParent(invPanel.transform, false);
            var hlgRect = hlgObj.AddComponent<RectTransform>();
            hlgRect.anchorMin = Vector2.zero; hlgRect.anchorMax = Vector2.one;
            hlgRect.offsetMin = new Vector2(50, 10); hlgRect.offsetMax = new Vector2(-50, -30);

            var hlg = hlgObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter; hlg.spacing = 30;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            TMPro.TextMeshProUGUI[] invTexts = new TMPro.TextMeshProUGUI[4];
            string[] invNames = { "TRÀ:\n10", "ĐƯỜNG:\n10", "LY:\n20", "ĐÁ:\n100%" };
            Sprite[] invSprites = { sprTea, sprSugar, sprCup, sprIce };

            for(int i=0; i<4; i++) {
                GameObject slot = new GameObject($"Slot_{i}");
                slot.transform.SetParent(hlgObj.transform, false);
                var le = slot.AddComponent<UnityEngine.UI.LayoutElement>();
                le.minWidth = 180; le.minHeight = 120;
                
                // Icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(slot.transform, false);
                var iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0, 0); iconRect.anchorMax = new Vector2(0, 1);
                iconRect.pivot = new Vector2(0, 0.5f);
                iconRect.anchoredPosition = new Vector2(0, 0);
                iconRect.sizeDelta = new Vector2(100, 100);
                
                var iconImg = iconObj.AddComponent<UnityEngine.UI.Image>();
                if (invSprites[i] != null) {
                    iconImg.sprite = invSprites[i];
                    iconImg.preserveAspect = true;
                }
                
                // Text
                var text = new GameObject("Text").AddComponent<TMPro.TextMeshProUGUI>();
                text.transform.SetParent(slot.transform, false);
                text.rectTransform.anchorMin = new Vector2(1, 0); text.rectTransform.anchorMax = new Vector2(1, 1);
                text.rectTransform.pivot = new Vector2(1, 0.5f);
                text.rectTransform.anchoredPosition = new Vector2(0, 0);
                text.rectTransform.sizeDelta = new Vector2(80, 100);
                
                text.text = invNames[i]; 
                text.fontSize = 24; 
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.color = new Color(0.2f, 0.2f, 0.2f);
                text.fontStyle = TMPro.FontStyles.Bold;
                invTexts[i] = text;
            }

            // Gán reference cho GameplayHUD qua Reflection
            gameplayHUD.GetType().GetField("playerNameText", bf).SetValue(gameplayHUD, nameText);
            gameplayHUD.GetType().GetField("energySlider", bf).SetValue(gameplayHUD, energySlider);
            gameplayHUD.GetType().GetField("stressSlider", bf).SetValue(gameplayHUD, stressSlider);
            gameplayHUD.GetType().GetField("moneyText", bf).SetValue(gameplayHUD, moneyText);
            gameplayHUD.GetType().GetField("clockText", bf).SetValue(gameplayHUD, clockText);
            gameplayHUD.GetType().GetField("teaCountText", bf).SetValue(gameplayHUD, invTexts[0]);
            gameplayHUD.GetType().GetField("sugarCountText", bf).SetValue(gameplayHUD, invTexts[1]);
            gameplayHUD.GetType().GetField("cupCountText", bf).SetValue(gameplayHUD, invTexts[2]);
            gameplayHUD.GetType().GetField("iceLevelText", bf).SetValue(gameplayHUD, invTexts[3]);

            GameObject promptObj = new GameObject("InteractionPrompt");
            promptObj.transform.SetParent(canvasObj.transform, false);
            var pRect = promptObj.AddComponent<RectTransform>();
            pRect.anchoredPosition = new Vector2(0, 150); // Nâng lên khỏi xe đẩy
            pRect.sizeDelta = new Vector2(160, 60);
            
            // Viền đỏ cho nút E
            promptObj.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.7f);
            var pOutline = promptObj.AddComponent<UnityEngine.UI.Outline>();
            pOutline.effectColor = Color.red; pOutline.effectDistance = new Vector2(3, 3);

            promptObj.AddComponent<CanvasGroup>();
            var promptUI = promptObj.AddComponent<InteractionPromptUI>();

            GameObject pTextObj = new GameObject("Text");
            pTextObj.transform.SetParent(promptObj.transform, false);
            var promptText = pTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            promptText.rectTransform.anchorMin = Vector2.zero; promptText.rectTransform.anchorMax = Vector2.one;
            promptText.rectTransform.sizeDelta = Vector2.zero;
            promptText.text = "Nhấn E";
            promptText.fontSize = 28; promptText.color = Color.white;
            promptText.alignment = TMPro.TextAlignmentOptions.Center;
            promptUI.GetType().GetField("promptText", bf).SetValue(promptUI, promptText);
            promptUI.GetType().GetField("canvasGroup", bf).SetValue(promptUI, promptObj.GetComponent<CanvasGroup>());

            // Day Summary UI (ẩn ban đầu)
            GameObject summaryObj = new GameObject("DaySummary");
            summaryObj.transform.SetParent(canvasObj.transform, false);
            var sumRect = summaryObj.AddComponent<RectTransform>();
            sumRect.anchorMin = Vector2.zero; sumRect.anchorMax = Vector2.one;
            sumRect.offsetMin = Vector2.zero; sumRect.offsetMax = Vector2.zero;
            summaryObj.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.85f);
            var sumCG = summaryObj.AddComponent<CanvasGroup>();
            sumCG.alpha = 0; sumCG.interactable = false; sumCG.blocksRaycasts = false;
            var summaryUI = summaryObj.AddComponent<DaySummaryUI>();

            var sumTitle = new GameObject("Title").AddComponent<TMPro.TextMeshProUGUI>();
            sumTitle.transform.SetParent(summaryObj.transform, false);
            sumTitle.rectTransform.anchoredPosition = new Vector2(0, 100);
            sumTitle.rectTransform.sizeDelta = new Vector2(400, 60);
            sumTitle.text = "Tổng Kết Ngày"; sumTitle.fontSize = 42;
            sumTitle.alignment = TMPro.TextAlignmentOptions.Center;

            var sumCustomers = new GameObject("Customers").AddComponent<TMPro.TextMeshProUGUI>();
            sumCustomers.transform.SetParent(summaryObj.transform, false);
            sumCustomers.rectTransform.anchoredPosition = new Vector2(0, 30);
            sumCustomers.rectTransform.sizeDelta = new Vector2(400, 40);
            sumCustomers.fontSize = 28; sumCustomers.alignment = TMPro.TextAlignmentOptions.Center;

            var sumMoney = new GameObject("Money").AddComponent<TMPro.TextMeshProUGUI>();
            sumMoney.transform.SetParent(summaryObj.transform, false);
            sumMoney.rectTransform.anchoredPosition = new Vector2(0, -20);
            sumMoney.rectTransform.sizeDelta = new Vector2(400, 40);
            sumMoney.fontSize = 28; sumMoney.alignment = TMPro.TextAlignmentOptions.Center;

            var sumStress = new GameObject("Stress").AddComponent<TMPro.TextMeshProUGUI>();
            sumStress.transform.SetParent(summaryObj.transform, false);
            sumStress.rectTransform.anchoredPosition = new Vector2(0, -70);
            sumStress.rectTransform.sizeDelta = new Vector2(400, 40);
            sumStress.fontSize = 28; sumStress.alignment = TMPro.TextAlignmentOptions.Center;

            var sumBtn = new GameObject("ContinueBtn");
            sumBtn.transform.SetParent(summaryObj.transform, false);
            var btnRect = sumBtn.AddComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -140); btnRect.sizeDelta = new Vector2(200, 50);
            sumBtn.AddComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.6f, 0.3f);
            var btn = sumBtn.AddComponent<UnityEngine.UI.Button>();
            var btnText = new GameObject("Text").AddComponent<TMPro.TextMeshProUGUI>();
            btnText.transform.SetParent(sumBtn.transform, false);
            btnText.rectTransform.anchorMin = Vector2.zero; btnText.rectTransform.anchorMax = Vector2.one;
            btnText.rectTransform.sizeDelta = Vector2.zero;
            btnText.text = "Ngày Tiếp Theo"; btnText.fontSize = 22;
            btnText.alignment = TMPro.TextAlignmentOptions.Center; btnText.color = Color.white;

            summaryUI.GetType().GetField("canvasGroup", bf).SetValue(summaryUI, sumCG);
            summaryUI.GetType().GetField("titleText", bf).SetValue(summaryUI, sumTitle);
            summaryUI.GetType().GetField("customersServedText", bf).SetValue(summaryUI, sumCustomers);
            summaryUI.GetType().GetField("moneyEarnedText", bf).SetValue(summaryUI, sumMoney);
            summaryUI.GetType().GetField("stressLevelText", bf).SetValue(summaryUI, sumStress);
            summaryUI.GetType().GetField("continueButton", bf).SetValue(summaryUI, btn);


            string sceneDir = "Assets/_Project/Scenes/Chapter1";
            if (!AssetDatabase.IsValidFolder(sceneDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Chapter1");
            }
            string scenePath = $"{sceneDir}/Chapter1.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[Gánh Hàng Rong] Đã tạo thành công {scenePath} - BẤM PLAY ĐỂ CHƠI!");
        }

        private static Material CreateMaterial(string name, string shaderName, Color color)
        {
            Shader shader = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            
            string path = $"Assets/_Project/Art/Materials/{name}.mat";
            if (!System.IO.Directory.Exists("Assets/_Project/Art/Materials"))
                System.IO.Directory.CreateDirectory("Assets/_Project/Art/Materials");
                
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static WeatherPreset CreateWeatherPreset(string name, WeatherType type, Color lightColor, float intensity, float rain, float wind, float fog, float customerMod)
        {
            WeatherPreset preset = ScriptableObject.CreateInstance<WeatherPreset>();
            preset.weatherType = type;
            preset.ambientLightColor = lightColor;
            preset.ambientLightIntensity = intensity;
            preset.rainIntensity = rain;
            preset.windStrength = wind;
            preset.fogDensity = fog;
            preset.customerSpawnModifier = customerMod;
            preset.iceMeltModifier = (rain > 0.5f) ? 0.5f : 1f;

            string path = $"Assets/_Project/ScriptableObjects/Weather/{name}.asset";
            if (!System.IO.Directory.Exists("Assets/_Project/ScriptableObjects/Weather"))
                System.IO.Directory.CreateDirectory("Assets/_Project/ScriptableObjects/Weather");

            AssetDatabase.CreateAsset(preset, path);
            return preset;
        }

        private static NPCProfile CreateNPCProfile(string name, NPCType type, float minPat, float maxPat, float minDrink, float maxDrink, float tip)
        {
            NPCProfile profile = ScriptableObject.CreateInstance<NPCProfile>();
            profile.npcType = type;
            profile.npcName = name;
            profile.minPatience = minPat;
            profile.maxPatience = maxPat;
            profile.minDrinkTime = minDrink;
            profile.maxDrinkTime = maxDrink;
            profile.tipChance = tip;
            
            // Random color tint for variety
            profile.possibleColorTints = new Color[] { 
                Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f),
                Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f) 
            };

            string path = $"Assets/_Project/ScriptableObjects/NPC/{name}.asset";
            if (!System.IO.Directory.Exists("Assets/_Project/ScriptableObjects/NPC"))
                System.IO.Directory.CreateDirectory("Assets/_Project/ScriptableObjects/NPC");

            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        private static void SetupPlayerAnimator(GameObject visualObj)
        {
            var animator = visualObj.GetComponent<Animator>();
            if (animator == null) animator = visualObj.AddComponent<Animator>();

            string controllerPath = "Assets/_Project/Animations/Player/PlayerAnimController.controller";
            
            // Luôn tạo lại controller để cập nhật animation mới
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations/Player"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Animations/Player");
                AssetDatabase.Refresh();
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("State", AnimatorControllerParameterType.Int);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var rootStateMachine = controller.layers[0].stateMachine;
            var idleState = rootStateMachine.AddState("Idle");
            var walkState = rootStateMachine.AddState("Walk");
            var runState = rootStateMachine.AddState("Run");
            var pushState = rootStateMachine.AddState("Pushing");
            var serveState = rootStateMachine.AddState("Serving");

            string modelDir = "Assets/_Project/Art/Models/Player/Meshy_AI_Ripped_Jeans_Portrait_biped";

            // Walk - dùng Walking animation
            AnimationClip walkClip = FindAnimClip($"{modelDir}/Meshy_AI_Ripped_Jeans_Portrait_biped_Animation_Walking_withSkin.glb");
            if (walkClip != null) walkState.motion = walkClip;

            // Run - dùng Running animation
            AnimationClip runClip = FindAnimClip($"{modelDir}/Meshy_AI_Ripped_Jeans_Portrait_biped_Animation_Running_withSkin.glb");
            if (runClip == null) runClip = FindAnimClip($"{modelDir}/Meshy_AI_Ripped_Jeans_Portrait_biped_Animation_Run_03_withSkin.glb");
            if (runClip != null) runState.motion = runClip;

            // Push Cart - dùng Unsteady Walk
            AnimationClip pushClip = FindAnimClip($"{modelDir}/Meshy_AI_Ripped_Jeans_Portrait_biped_Animation_Unsteady_Walk_withSkin.glb");
            if (pushClip != null) pushState.motion = pushClip;

            // Idle - dùng Walking clip ở speed 0 nếu không có idle riêng (hoặc sẽ dùng procedural)
            // Serving - procedural

            // Tạo Transitions MƯỢT MÀ
            // hasExitTime = false -> chuyển state ngay lập tức
            // transitionDuration = 0.15 -> blend mượt trong 0.15 giây
            var anyToIdle = rootStateMachine.AddAnyStateTransition(idleState);
            anyToIdle.AddCondition(AnimatorConditionMode.Equals, 0, "State");
            anyToIdle.hasExitTime = false;
            anyToIdle.duration = 0.15f;
            anyToIdle.canTransitionToSelf = false;

            var anyToWalk = rootStateMachine.AddAnyStateTransition(walkState);
            anyToWalk.AddCondition(AnimatorConditionMode.Equals, 1, "State");
            anyToWalk.hasExitTime = false;
            anyToWalk.duration = 0.15f;
            anyToWalk.canTransitionToSelf = false;

            var anyToRun = rootStateMachine.AddAnyStateTransition(runState);
            anyToRun.AddCondition(AnimatorConditionMode.Equals, 2, "State");
            anyToRun.hasExitTime = false;
            anyToRun.duration = 0.15f;
            anyToRun.canTransitionToSelf = false;

            var anyToPush = rootStateMachine.AddAnyStateTransition(pushState);
            anyToPush.AddCondition(AnimatorConditionMode.Equals, 3, "State");
            anyToPush.hasExitTime = false;
            anyToPush.duration = 0.2f;
            anyToPush.canTransitionToSelf = false;

            var anyToServe = rootStateMachine.AddAnyStateTransition(serveState);
            anyToServe.AddCondition(AnimatorConditionMode.Equals, 4, "State");
            anyToServe.hasExitTime = false;
            anyToServe.duration = 0.1f;
            anyToServe.canTransitionToSelf = false;
            
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
        }

        private static AnimationClip FindAnimClip(string glbPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(glbPath);
            if (assets == null) return null;
            foreach (var a in assets)
            {
                if (a is AnimationClip c && !c.name.StartsWith("__preview"))
                    return c;
            }
            return null;
        }

        private static Sprite LoadSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (importer != null && importer.textureType != UnityEditor.TextureImporterType.Sprite)
            {
                importer.textureType = UnityEditor.TextureImporterType.Sprite;
                importer.spriteImportMode = UnityEditor.SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
