using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace GanhHangRong.Editor
{
    public static class MainMenuSceneBuilder
    {
        [MenuItem("Gánh Hàng Rong/Dựng Scene Main Menu", false, 11)]
        public static void BuildMainMenuScene()
        {
            // Tạo scene mới
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "MainMenu";

            // 1. Camera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            // 2. EventSystem
            GameObject evtSystem = new GameObject("EventSystem");
            evtSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Sử dụng InputSystemUIInputModule cho hệ thống Input System mới thay vì StandaloneInputModule cũ
            evtSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 3. Canvas (UI)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f; // Cân bằng Scale
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Script Menu
            var menuUI = canvasObj.AddComponent<UI.MainMenuUI>();

            // 4. Background (Cấu hình hình ảnh mới copy)
            string bgPath = "Assets/_Project/Art/UI/MainMenuBG.jpg";
            AssetDatabase.Refresh(); // Cập nhật Unity Asset Database để nhận diện ảnh vừa chép
            TextureImporter importer = AssetImporter.GetAtPath(bgPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgImg = bgObj.AddComponent<Image>();
            
            // Xử lý tạo Sprite an toàn kể cả khi nó chỉ là Texture2D chưa được import thành Sprite
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            if (bgSprite != null) 
            {
                bgImg.sprite = bgSprite;
            }
            else 
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(bgPath);
                if (tex != null)
                {
                    bgSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    bgImg.sprite = bgSprite;
                }
                else 
                {
                    bgImg.color = new Color(0.1f, 0.15f, 0.2f); // Màu thay thế nếu thiếu ảnh
                }
            }

            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Gradient che nửa trên cho rõ chữ
            GameObject topGradient = new GameObject("TopGradient");
            topGradient.transform.SetParent(canvasObj.transform, false);
            var gradImg = topGradient.AddComponent<Image>();
            gradImg.color = new Color(0, 0, 0, 0.45f); // Làm mờ đen 45%
            var gradRect = topGradient.GetComponent<RectTransform>();
            gradRect.anchorMin = new Vector2(0, 0.65f);
            gradRect.anchorMax = new Vector2(1, 1f);
            gradRect.sizeDelta = Vector2.zero;

            // 5. Tiêu đề "GÁNH HÀNG RONG"
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(canvasObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "GÁNH HÀNG RONG";
            titleText.fontSize = 120;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.95f, 0.75f, 0.35f); // Màu vàng ấm
            // Thêm viền / bóng để nổi bật
            titleText.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
            titleText.fontSharedMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.8f));
            titleText.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 2);
            titleText.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -2);

            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -180);
            titleRect.sizeDelta = new Vector2(1200, 150);

            // 6. Subtitle
            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(titleObj.transform, false);
            var subText = subObj.AddComponent<TextMeshProUGUI>();
            subText.text = "Ánh Đèn Đêm Miền Tây | Hành Trình Mưu Sinh Nơi Miền Biển\nDành cả tuổi trẻ chỉ để giữ một ánh đèn còn sáng giữa đêm.";
            subText.fontSize = 32;
            subText.fontStyle = FontStyles.Italic;
            subText.alignment = TextAlignmentOptions.Center;
            subText.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            
            var subRect = subObj.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0f);
            subRect.anchorMax = new Vector2(0.5f, 0f);
            subRect.anchoredPosition = new Vector2(0, -80);
            subRect.sizeDelta = new Vector2(1400, 100);

            // 7. Bảng Nút (Horizontal Layout)
            GameObject panelObj = new GameObject("ButtonPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.anchoredPosition = new Vector2(0, 80); // Cách đáy 80px
            panelRect.sizeDelta = new Vector2(0, 100);

            var hLayout = panelObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 30;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // 8. Khởi tạo 6 nút
            CreateMenuButton(panelObj.transform, "BẮT ĐẦU CHƠI", menuUI, "OnPlayClicked");
            CreateMenuButton(panelObj.transform, "TIẾP TỤC", menuUI, "OnContinueClicked");
            CreateMenuButton(panelObj.transform, "CÀI ĐẶT", menuUI, "OnSettingsClicked");
            CreateMenuButton(panelObj.transform, "THÀNH TÍCH", menuUI, "OnAchievementsClicked");
            CreateMenuButton(panelObj.transform, "VỀ GAME", menuUI, "OnAboutClicked");
            CreateMenuButton(panelObj.transform, "THOÁT", menuUI, "OnQuitClicked");

            // Lưu scene
            string scenePath = "Assets/_Project/Scenes/MainMenu/MainMenu.unity";
            if (!System.IO.Directory.Exists("Assets/_Project/Scenes/MainMenu"))
                System.IO.Directory.CreateDirectory("Assets/_Project/Scenes/MainMenu");

            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[Gánh Hàng Rong] Đã tạo thành công {scenePath} với giao diện đầy đủ!");
        }

        private static void CreateMenuButton(Transform parent, string text, UI.MainMenuUI menuUI, string methodName)
        {
            GameObject btnObj = new GameObject(text + "_Button");
            btnObj.transform.SetParent(parent, false);
            
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(240, 80); // Kích thước mỗi nút

            // Nền nút giả lập màu gỗ
            var bgImg = btnObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.1f, 0.05f, 0.85f); 

            // Viền vàng giả lập bảng hiệu
            var outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.6f, 0.3f, 0.8f);
            outline.effectDistance = new Vector2(3, -3);

            var btn = btnObj.AddComponent<Button>();
            
            // Text của nút
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.fontSize = 26;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = new Color(0.9f, 0.85f, 0.7f); // Vàng sáng
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Đăng ký sự kiện
            var actionDelegate = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), menuUI, methodName) as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, actionDelegate);
        }
    }
}
