using UnityEngine;

namespace StreetLegends.Gameplay.Character
{
    public class CharacterActions
    {
        private readonly CharacterRuntimeStats _stats;
        private float _tackleTimer;
        private bool _isTackling;

        public bool CanTackle => _tackleTimer <= 0f && !_isTackling;
        public bool IsTackling => _isTackling;
        public float TackleTimer => Mathf.Max(0f, _tackleTimer);
        public float TackleProgress => _isTackling ? 1f - Mathf.Clamp01((_stats.TackleCooldown - _tackleTimer) / _stats.TackleDuration) : 0f;

        public CharacterActions(CharacterRuntimeStats stats)
        {
            _stats = stats;
        }

        public void Update(float deltaTime)
        {
            if (_tackleTimer > 0f)
            {
                _tackleTimer -= deltaTime;
                if (_tackleTimer <= _stats.TackleCooldown - _stats.TackleDuration)
                {
                    _isTackling = false;
                }
            }
        }

        public bool TryTackle()
        {
            if (!CanTackle) return false;
            _isTackling = true;
            _tackleTimer = _stats.TackleCooldown;
            return true;
        }

        public void Reset()
        {
            _tackleTimer = 0f;
            _isTackling = false;
        }
    }
}
