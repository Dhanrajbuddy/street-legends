using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StreetLegends.Core;

namespace StreetLegends.Presentation.Screens
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private void Start()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            SceneManager.LoadScene(SceneNames.Match);
        }
    }
}
