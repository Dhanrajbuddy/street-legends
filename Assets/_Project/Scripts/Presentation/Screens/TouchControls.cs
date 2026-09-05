using UnityEngine;
using UnityEngine.UI;
using StreetLegends.Gameplay.Input;

namespace StreetLegends.Presentation.Screens
{
    public class TouchControls : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private TouchInputSource inputSource;
        [SerializeField] private Button kickButton;
        [SerializeField] private Button tackleButton;
        [SerializeField] private SprintButton sprintButton;

        private bool _kickPressed;
        private bool _tacklePressed;

        private void Start()
        {
            if (kickButton != null)
            {
                kickButton.onClick.AddListener(OnKickPressed);
            }
            if (tackleButton != null)
            {
                tackleButton.onClick.AddListener(OnTacklePressed);
            }
        }

        private void OnDestroy()
        {
            if (kickButton != null)
                kickButton.onClick.RemoveListener(OnKickPressed);
            if (tackleButton != null)
                tackleButton.onClick.RemoveListener(OnTacklePressed);
        }

        private void Update()
        {
            if (inputSource == null) return;

            inputSource.MoveInput = joystick != null ? joystick.Direction : Vector2.zero;
            inputSource.KickPressed = _kickPressed;
            inputSource.TacklePressed = _tacklePressed;
            inputSource.SprintHeld = sprintButton != null && sprintButton.IsHeld;

            _kickPressed = false;
            _tacklePressed = false;
        }

        private void OnKickPressed()
        {
            _kickPressed = true;
        }

        private void OnTacklePressed()
        {
            _tacklePressed = true;
        }
    }
}
