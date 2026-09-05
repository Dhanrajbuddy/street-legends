using UnityEngine;
using UnityEngine.InputSystem;
using StreetLegends.Gameplay.Input;

namespace StreetLegends.Gameplay.Input
{
    public class KeyboardInputSource : MonoBehaviour, IInputSource
    {
        public Vector2 MoveInput { get; private set; }
        public bool KickPressed { get; private set; }
        public bool TacklePressed { get; private set; }
        public bool SprintHeld { get; private set; }

        private bool _kickWasPressed;
        private bool _tackleWasPressed;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                MoveInput = Vector2.zero;
                KickPressed = false;
                TacklePressed = false;
                SprintHeld = false;
                return;
            }

            Vector2 move = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
            MoveInput = move.sqrMagnitude > 1f ? move.normalized : move;

            SprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

            bool kickNow = keyboard.spaceKey.isPressed;
            KickPressed = kickNow && !_kickWasPressed;
            _kickWasPressed = kickNow;

            bool tackleNow = keyboard.eKey.isPressed || keyboard.qKey.isPressed;
            TacklePressed = tackleNow && !_tackleWasPressed;
            _tackleWasPressed = tackleNow;
        }
    }
}
