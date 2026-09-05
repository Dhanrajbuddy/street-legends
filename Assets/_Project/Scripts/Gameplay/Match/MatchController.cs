using System;
using UnityEngine;
using StreetLegends.Data.Match;
using StreetLegends.Gameplay.Match;
using StreetLegends.Gameplay.Ball;
using StreetLegends.Gameplay.Court;
using StreetLegends.Gameplay.Character;
using StreetLegends.Gameplay.Ai;

namespace StreetLegends.Gameplay.Match
{
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private MatchConfig matchConfig;
        [SerializeField] private AiDifficultyConfig aiDifficultyConfig;
        [SerializeField] private CourtBehaviour court;
        [SerializeField] private BallBehaviour ball;
        [SerializeField] private CharacterBehaviour player;
        [SerializeField] private CharacterBehaviour ai;
        [SerializeField] private GoalTrigger playerGoalTrigger;
        [SerializeField] private GoalTrigger aiGoalTrigger;
        [SerializeField] private AiInputSource aiInputSource;

        public MatchState State { get; private set; } = MatchState.Countdown;
        public MatchResult Result { get; private set; } = MatchResult.None;
        public ScoreBoard ScoreBoard { get; } = new ScoreBoard();
        public MatchClock Clock { get; } = new MatchClock();

        public event Action<MatchState> StateChanged;
        public event Action<int, int> ScoreChanged;
        public event Action<MatchResult> MatchFinished;
        public event Action<int> CountdownTick;
        public event Action<float> TimerTick;

        private float _goalCelebrationTimer;
        private int _lastCountdownSecond = -1;

        private void Start()
        {
            if (playerGoalTrigger != null)
                playerGoalTrigger.OnGoalScored += HandleGoalScored;
            if (aiGoalTrigger != null)
                aiGoalTrigger.OnGoalScored += HandleGoalScored;

            BeginCountdown();
        }

        private void OnDestroy()
        {
            if (playerGoalTrigger != null)
                playerGoalTrigger.OnGoalScored -= HandleGoalScored;
            if (aiGoalTrigger != null)
                aiGoalTrigger.OnGoalScored -= HandleGoalScored;
        }

        private void Update()
        {
            switch (State)
            {
                case MatchState.Countdown:
                    UpdateCountdown();
                    break;
                case MatchState.Playing:
                    UpdatePlaying();
                    break;
                case MatchState.GoalScored:
                    UpdateGoalScored();
                    break;
                case MatchState.Overtime:
                    UpdateOvertime();
                    break;
            }
        }

        private void BeginCountdown()
        {
            State = MatchState.Countdown;
            Clock.StartCountdown(matchConfig != null ? matchConfig.countdownDurationSeconds : 3f);
            ResetPositions();
            StateChanged?.Invoke(State);
        }

        private void UpdateCountdown()
        {
            Clock.TickCountdown(Time.deltaTime);

            int currentSecond = Mathf.CeilToInt(Clock.CountdownTime);
            if (currentSecond != _lastCountdownSecond)
            {
                _lastCountdownSecond = currentSecond;
                CountdownTick?.Invoke(currentSecond);
            }

            if (Clock.IsCountdownComplete)
            {
                BeginPlaying();
            }
        }

        private void BeginPlaying()
        {
            State = MatchState.Playing;
            Clock.StartMatch(matchConfig != null ? matchConfig.matchDurationSeconds : 120f);
            StateChanged?.Invoke(State);
        }

        private void UpdatePlaying()
        {
            Clock.Tick(Time.deltaTime);
            TimerTick?.Invoke(Clock.RemainingTime);

            if (Clock.IsTimeUp)
            {
                if (MatchRules.ShouldEnterOvertime(ScoreBoard.PlayerScore, ScoreBoard.AiScore, matchConfig))
                {
                    BeginOvertime();
                }
                else
                {
                    FinishMatch();
                }
            }
        }

        private void BeginOvertime()
        {
            State = MatchState.Overtime;
            float otDuration = MatchRules.GetOvertimeDuration(matchConfig);
            if (otDuration > 0f)
            {
                Clock.StartMatch(otDuration);
            }
            else
            {
                Clock.StartMatch(999f);
            }
            StateChanged?.Invoke(State);
        }

        private void UpdateOvertime()
        {
            Clock.Tick(Time.deltaTime);
            TimerTick?.Invoke(Clock.RemainingTime);

            float otDuration = MatchRules.GetOvertimeDuration(matchConfig);
            if (otDuration > 0f && Clock.IsTimeUp)
            {
                FinishMatch();
            }
        }

        private void HandleGoalScored(GoalSide side)
        {
            if (State != MatchState.Playing && State != MatchState.Overtime) return;

            if (side == GoalSide.PlayerGoal)
            {
                ScoreBoard.AiGoal();
            }
            else
            {
                ScoreBoard.PlayerGoal();
            }

            ScoreChanged?.Invoke(ScoreBoard.PlayerScore, ScoreBoard.AiScore);

            if (MatchRules.IsScoreCapReached(ScoreBoard.PlayerScore, ScoreBoard.AiScore, matchConfig))
            {
                FinishMatch();
                return;
            }

            if (State == MatchState.Overtime)
            {
                FinishMatch();
                return;
            }

            State = MatchState.GoalScored;
            _goalCelebrationTimer = matchConfig != null ? matchConfig.goalCelebrationDurationSeconds : 2f;
            StateChanged?.Invoke(State);
        }

        private void UpdateGoalScored()
        {
            _goalCelebrationTimer -= Time.deltaTime;
            if (_goalCelebrationTimer <= 0f)
            {
                ResetPositions();
                BeginPlaying();
            }
        }

        private void FinishMatch()
        {
            State = MatchState.Finished;
            Clock.Stop();
            Result = MatchRules.DetermineResult(ScoreBoard.PlayerScore, ScoreBoard.AiScore);
            StateChanged?.Invoke(State);
            MatchFinished?.Invoke(Result);
        }

        private void ResetPositions()
        {
            if (court != null)
            {
                player?.ResetPosition(court.PlayerStart);
                ai?.ResetPosition(court.AiStart);
                ball?.Reset(court.BallStart);
            }
            aiInputSource?.Reset();
        }

        public void RestartMatch()
        {
            ScoreBoard.Reset();
            Result = MatchResult.None;
            _lastCountdownSecond = -1;
            BeginCountdown();
        }
    }
}
