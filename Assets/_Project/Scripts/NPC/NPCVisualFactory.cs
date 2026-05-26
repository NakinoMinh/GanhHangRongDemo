using UnityEngine;
using GanhHangRong.Core;

namespace GanhHangRong.NPC
{
    /// <summary>
    /// Tạo visual cho NPC (dùng hình khối primitive nếu chưa có model) và animation thủ tục.
    /// </summary>
    public class NPCVisualFactory : MonoBehaviour
    {
        [Header("Materials")]
        [SerializeField] private Material baseMaterial;

        // Bảng màu cho từng loại NPC
        private Color colorFisherman = new Color(0.2f, 0.4f, 0.8f);    // Xanh dương
        private Color colorWorker = new Color(0.6f, 0.4f, 0.2f);       // Nâu
        private Color colorBusDriver = new Color(0.5f, 0.5f, 0.5f);    // Xám
        private Color colorIslandTraveler = new Color(0.3f, 0.8f, 0.4f); // Xanh lá
        private Color colorResident = new Color(0.9f, 0.9f, 0.8f);     // Trắng kem

        public GameObject CreateNPCVisual(NPCType type, Transform parent)
        {
            if (baseMaterial == null)
            {
                baseMaterial = new Material(Shader.Find("Standard"));
            }

            // Tạo model giả bằng primitive shapes
            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(parent);
            visualRoot.transform.localPosition = Vector3.zero;

            // Thân (Capsule)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Destroy(body.GetComponent<CapsuleCollider>());
            body.transform.SetParent(visualRoot.transform);
            body.transform.localPosition = new Vector3(0, 0.5f, 0); // Kéo lên khỏi mặt đất
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Đầu (Sphere)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(head.GetComponent<SphereCollider>());
            head.transform.SetParent(visualRoot.transform);
            head.transform.localPosition = new Vector3(0, 1.2f, 0);
            head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            // Mắt (Cube) - để biết NPC đang quay mặt hướng nào
            GameObject eyes = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(eyes.GetComponent<BoxCollider>());
            eyes.transform.SetParent(head.transform);
            eyes.transform.localPosition = new Vector3(0.1f, 0.1f, 0.15f);
            eyes.transform.localScale = new Vector3(0.3f, 0.1f, 0.1f);
            eyes.GetComponent<MeshRenderer>().material.color = Color.black;

            // Đổi màu theo loại NPC
            Color bodyColor = Color.white;
            switch (type)
            {
                case NPCType.Fisherman: bodyColor = colorFisherman; break;
                case NPCType.Worker: bodyColor = colorWorker; break;
                case NPCType.BusDriver: bodyColor = colorBusDriver; break;
                case NPCType.IslandTraveler: bodyColor = colorIslandTraveler; break;
                case NPCType.LocalResident: bodyColor = colorResident; break;
            }

            Material bodyMat = new Material(baseMaterial);
            bodyMat.color = bodyColor;
            body.GetComponent<MeshRenderer>().material = bodyMat;

            Material headMat = new Material(baseMaterial);
            headMat.color = new Color(1f, 0.8f, 0.6f); // Màu da
            head.GetComponent<MeshRenderer>().material = headMat;

            // Thêm Procedural Animator
            var proceduralAnim = visualRoot.AddComponent<NPCProceduralAnimator>();
            return visualRoot;
        }
    }

    /// <summary>
    /// Component gắn vào VisualRoot của NPC để tạo hiệu ứng đung đưa (thay cho Animator).
    /// </summary>
    public class NPCProceduralAnimator : MonoBehaviour
    {
        private NPCController controller;
        private float animTimer;
        private Vector3 originalPos;

        private void Start()
        {
            controller = GetComponentInParent<NPCController>();
            originalPos = transform.localPosition;
        }

        private void Update()
        {
            if (controller == null) return;
            animTimer += Time.deltaTime;

            if (controller.CurrentState == NPCState.WalkingIn || controller.CurrentState == NPCState.WalkingOut)
            {
                // Đi bộ - Lắc qua lắc lại, nhấp nhô
                float bobY = Mathf.Abs(Mathf.Sin(animTimer * 10f)) * 0.1f;
                float tiltZ = Mathf.Sin(animTimer * 5f) * 10f;
                transform.localPosition = originalPos + new Vector3(0, bobY, 0);
                transform.localRotation = Quaternion.Euler(0, 0, tiltZ);
            }
            else if (controller.CurrentState == NPCState.SittingDown || controller.CurrentState == NPCState.Waiting || controller.CurrentState == NPCState.Drinking)
            {
                // Ngồi - Hơi thụt xuống, thở nhẹ
                float breathe = Mathf.Sin(animTimer * 2f) * 0.02f;
                transform.localPosition = originalPos + new Vector3(0, -0.3f + breathe, 0);
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                // Idle
                transform.localPosition = originalPos;
                transform.localRotation = Quaternion.identity;
            }
        }
    }
}
