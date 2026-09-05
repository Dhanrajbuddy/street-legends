using UnityEngine;
using StreetLegends.Data.Match;

namespace StreetLegends.Gameplay.Ai
{
    public enum AiAction
    {
        Idle,
        ChaseBall,
        DefendGoal,
        AttackWithBall,
        PressOpponent,
        Tackle
    }

    public struct AiGameState
    {
        public Vector3 BallPosition;
        public Vector3 OwnPosition;
        public Vector3 OpponentPosition;
        public Vector3 OwnGoalPosition;
        public Vector3 OpponentGoalPosition;
        public bool BallIsPossessedBySelf;
        public bool BallIsPossessedByOpponent;
        public bool BallIsFree;
        public float DistanceToBall;
        public float OpponentDistanceToBall;
        public float DistanceToOwnGoal;
        public float DistanceToOpponentGoal;
        public float DistanceToOpponent;
    }

    public class AiBrain
    {
        private readonly AiDifficultyConfig _config;
        private AiAction _currentAction = AiAction.Idle;
        private float _decisionTimer;
        private Vector3 _lastBallPos;

        public AiAction CurrentAction => _currentAction;
        public float DecisionTimer => _decisionTimer;

        public AiBrain(AiDifficultyConfig config)
        {
            _config = config;
        }

        public AiAction Decide(AiGameState state)
        {
            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer > 0f) return _currentAction;

            _decisionTimer = _config.decisionIntervalSeconds;

            float chaseScore = ScoreChaseBall(state);
            float defendScore = ScoreDefendGoal(state);
            float attackScore = ScoreAttackWithBall(state);
            float pressScore = ScorePressOpponent(state);
            float tackleScore = ScoreTackle(state);

            if (Random.value < _config.errorRate)
            {
                _currentAction = (AiAction)Random.Range(0, 6);
                return _currentAction;
            }

            float bestScore = chaseScore;
            AiAction bestAction = AiAction.ChaseBall;

            if (defendScore > bestScore) { bestScore = defendScore; bestAction = AiAction.DefendGoal; }
            if (attackScore > bestScore) { bestScore = attackScore; bestAction = AiAction.AttackWithBall; }
            if (pressScore > bestScore) { bestScore = pressScore; bestAction = AiAction.PressOpponent; }
            if (tackleScore > bestScore) { bestScore = tackleScore; bestAction = AiAction.Tackle; }

            _currentAction = bestAction;
            return _currentAction;
        }

        public void Reset()
        {
            _currentAction = AiAction.Idle;
            _decisionTimer = 0f;
        }

        private float ScoreChaseBall(AiGameState state)
        {
            if (!state.BallIsFree) return 0f;
            float proximity = 1f / (1f + state.DistanceToBall * 0.3f);
            float opponentFactor = state.OpponentDistanceToBall > state.DistanceToBall ? 0.3f : 0f;
            return proximity + opponentFactor + _config.aggression * 0.3f;
        }

        private float ScoreDefendGoal(AiGameState state)
        {
            if (state.BallIsPossessedBySelf) return 0f;
            float threatLevel = state.BallIsPossessedByOpponent ? 1f : 0.3f;
            float goalProximity = 1f / (1f + state.DistanceToOwnGoal * 0.2f);
            return threatLevel * goalProximity * _config.defenseAwareness + 0.2f;
        }

        private float ScoreAttackWithBall(AiGameState state)
        {
            if (!state.BallIsPossessedBySelf) return 0f;
            float goalProximity = 1f / (1f + state.DistanceToOpponentGoal * 0.15f);
            return goalProximity + _config.aggression * 0.5f + 0.5f;
        }

        private float ScorePressOpponent(AiGameState state)
        {
            if (!state.BallIsPossessedByOpponent) return 0f;
            float proximity = 1f / (1f + state.DistanceToOpponent * 0.4f);
            return proximity * _config.aggression + 0.1f;
        }

        private float ScoreTackle(AiGameState state)
        {
            if (!state.BallIsPossessedByOpponent) return 0f;
            if (state.DistanceToOpponent > 3f) return 0f;
            return 1f / (1f + state.DistanceToOpponent * 0.5f) * _config.aggression + 0.3f;
        }
    }
}
