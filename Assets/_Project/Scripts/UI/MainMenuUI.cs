using UnityEngine;
using UnityEngine.SceneManagement;
using GanhHangRong.Core;

namespace GanhHangRong.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            // Transition sang màn chơi chính
            StartCoroutine(LoadChapter1());
        }

        public void OnContinueClicked()
        {
            Debug.Log("[MainMenu] Continue clicked");
        }

        public void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings clicked");
        }

        public void OnAchievementsClicked()
        {
            Debug.Log("[MainMenu] Achievements clicked");
        }

        public void OnAboutClicked()
        {
            Debug.Log("[MainMenu] About clicked");
        }

        public void OnQuitClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private System.Collections.IEnumerator LoadChapter1()
        {
            // Gợi TransitionUI nếu có
            var transition = FindAnyObjectByType<TransitionUI>();
            if (transition != null)
            {
                yield return transition.FadeOut(1f);
            }

            SceneManager.LoadScene("Chapter1");
        }
    }
}
