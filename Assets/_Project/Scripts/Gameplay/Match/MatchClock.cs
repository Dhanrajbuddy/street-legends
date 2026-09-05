using UnityEngine;

namespace StreetLegends.Gameplay.Match
{
    public class MatchClock
    {
        private float _remainingTime;
        private float _countdownTime;
        private bool _isRunning;

        public float RemainingTime => Mathf.Max(0f, _remainingTime);
        public float CountdownTime => Mathf.Max(0f, _countdownTime);
        public bool IsRunning => _isRunning;
        public bool IsCountdownComplete => _countdownTime <= 0f;
        public bool IsTimeUp => _remainingTime <= 0f;

        public void StartCountdown(float duration)
        {
            _countdownTime = duration;
            _isRunning = true;
        }

        public void TickCountdown(float delta)
        {
            if (!_isRunning) return;
            _countdownTime -= delta;
            if (_countdownTime <= 0f)
            {
                _countdownTime = 0f;
                _isRunning = false;
            }
        }

        public void StartMatch(float duration)
        {
            _remainingTime = duration;
            _isRunning = true;
        }

        public void Tick(float delta)
        {
            if (!_isRunning) return;
            _remainingTime -= delta;
            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                _isRunning = false;
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Reset()
        {
            _remainingTime = 0f;
            _countdownTime = 0f;
            _isRunning = false;
        }
    }
}
