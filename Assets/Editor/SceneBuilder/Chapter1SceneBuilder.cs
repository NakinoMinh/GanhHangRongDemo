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
            
            string modelPath = "Assets/_Project/Art/Characters/Meshy_AI_Ripped_Jeans_Portrait_0525044139_texture.glb";
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
                GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visualObj.GetComponent<MeshRenderer>().sharedMaterial = matPlayer;
                Object.DestroyImmediate(visualObj.GetComponent<CapsuleCollider>());
                visualObj.transform.SetParent(playerObj.transform);
                visualObj.transform.localPosition = Vector3.zero;
            }
            
            var col2d = playerObj.AddComponent<CapsuleCollider2D>();
            col2d.size = new Vector2(0.5f, 1.8f);
            
            playerObj.AddComponent<Rigidbody2D>().freezeRotation = true;
            playerObj.AddComponent<PlayerController2D>();
            playerObj.AddComponent<PlayerAnimator>();
            playerObj.AddComponent<PlayerStats>();
            cinCam.SetTarget(playerObj.transform); // Set camera target

            // ==========================================
            // 6. CÁC HỆ THỐNG QUẢN LÝ (MANAGERS)
            // ==========================================
            GameObject managersObj = new GameObject("Managers");
            managersObj.AddComponent<GameManager>();
            var weatherMgr = managersObj.AddComponent<WeatherManager>();
            managersObj.AddComponent<RainSystem>();
            managersObj.AddComponent<WindSystem>();
            managersObj.AddComponent<FogController>();
            managersObj.AddComponent<EconomyManager>();
            managersObj.AddComponent<DayNightCycle>();
            managersObj.AddComponent<Narrative.DialogueManager>();
            managersObj.AddComponent<AudioManager>();
            managersObj.AddComponent<GanhHangRong.Systems.EmotionalFailureSystem>();

            // Thiết lập field cho WeatherManager (hack qua reflection vì đang trong editor)
            weatherMgr.GetType().GetField("globalLight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(weatherMgr, dirLight);

            // ==========================================
            // 7. HỆ THỐNG NPC SPAWNER
            // ==========================================
            GameObject spawnerObj = new GameObject("NPC_Spawner");
            var spawner = spawnerObj.AddComponent<NPCSpawner>();
            
            GameObject spawnLeft = new GameObject("SpawnPoint_Left");
            spawnLeft.transform.position = new Vector3(-20, 1, 0);
            spawnLeft.transform.SetParent(spawnerObj.transform);
            
            GameObject spawnRight = new GameObject("SpawnPoint_Right");
            spawnRight.transform.position = new Vector3(20, 1, 0);
            spawnRight.transform.SetParent(spawnerObj.transform);
            
            spawner.GetType().GetField("spawnPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(spawner, new Transform[] { spawnLeft.transform, spawnRight.transform });
            spawner.GetType().GetField("exitPoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(spawner, new Transform[] { spawnLeft.transform, spawnRight.transform });

            // ==========================================
            // 8. HỆ THỐNG UI
            // ==========================================
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            GameObject hudObj = new GameObject("HUD");
            hudObj.transform.SetParent(canvasObj.transform, false);
            hudObj.AddComponent<CanvasGroup>();
            hudObj.AddComponent<HUDManager>();
            // (Trong thực tế cần tạo TextMeshProUGUI và gán, ở script này tạm thời bỏ qua chi tiết UI phức tạp)

            GameObject promptObj = new GameObject("InteractionPrompt");
            promptObj.transform.SetParent(canvasObj.transform, false);
            promptObj.AddComponent<CanvasGroup>();
            var promptUI = promptObj.AddComponent<InteractionPromptUI>();
            var promptText = promptObj.AddComponent<TMPro.TextMeshProUGUI>();
            promptText.text = "Nhấn E";
            promptText.alignment = TMPro.TextAlignmentOptions.Center;
            promptUI.GetType().GetField("promptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(promptUI, promptText);
            promptUI.GetType().GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(promptUI, promptObj.GetComponent<CanvasGroup>());


            string scenePath = "Assets/_Project/Scenes/Chapter1/Chapter1.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[Gánh Hàng Rong] Đã tạo thành công {scenePath} - BẤM PLAY ĐỂ CHƠI!");
        }

        private static Material CreateMaterial(string name, string shaderName, Color color)
        {
            Shader shader = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(shader);
            mat.SetColor("_BaseColor", color);
            
            string path = $"Assets/_Project/Art/Materials/{name}.mat";
            if (!System.IO.Directory.Exists("Assets/_Project/Art/Materials"))
                System.IO.Directory.CreateDirectory("Assets/_Project/Art/Materials");
                
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void SetupPlayerAnimator(GameObject visualObj)
        {
            var animator = visualObj.GetComponent<Animator>();
            if (animator == null) animator = visualObj.AddComponent<Animator>();

            string controllerPath = "Assets/_Project/Animations/Player/PlayerAnimController.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            
            if (controller == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations/Player"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Animations/Player");
                    AssetDatabase.Refresh();
                }

                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("State", AnimatorControllerParameterType.Int);

                var rootStateMachine = controller.layers[0].stateMachine;
                var idleState = rootStateMachine.AddState("Idle");
                var walkState = rootStateMachine.AddState("Walk");
                var pushState = rootStateMachine.AddState("Pushing");
                var serveState = rootStateMachine.AddState("Serving");

                // Lấy animation clip từ file GLB
                Object[] walkAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Animations/Player/Meshy_AI_biped/Meshy_AI_biped_Animation_Walking_withSkin.glb");
                AnimationClip walkClip = null;
                if (walkAssets != null) {
                    foreach (var a in walkAssets) {
                        if (a is AnimationClip c && !c.name.StartsWith("__preview")) { walkClip = c; break; }
                    }
                }
                
                if (walkClip != null) walkState.motion = walkClip;

                // Tạo Transition
                var anyToIdle = rootStateMachine.AddAnyStateTransition(idleState);
                anyToIdle.AddCondition(AnimatorConditionMode.Equals, 0, "State");
                
                var anyToWalk = rootStateMachine.AddAnyStateTransition(walkState);
                anyToWalk.AddCondition(AnimatorConditionMode.Equals, 1, "State");

                var anyToPush = rootStateMachine.AddAnyStateTransition(pushState);
                anyToPush.AddCondition(AnimatorConditionMode.Equals, 2, "State");

                var anyToServe = rootStateMachine.AddAnyStateTransition(serveState);
                anyToServe.AddCondition(AnimatorConditionMode.Equals, 3, "State");
            }
            
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
        }
    }
}
