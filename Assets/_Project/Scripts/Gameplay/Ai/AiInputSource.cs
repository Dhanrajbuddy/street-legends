using UnityEngine;
using StreetLegends.Data.Match;
using StreetLegends.Gameplay.Input;
using StreetLegends.Gameplay.Ball;
using StreetLegends.Gameplay.Court;
using StreetLegends.Gameplay.Character;

namespace StreetLegends.Gameplay.Ai
{
    public class AiInputSource : MonoBehaviour, IInputSource
    {
        [SerializeField] private AiDifficultyConfig difficultyConfig;
        [SerializeField] private CharacterBehaviour opponent;
        [SerializeField] private CourtBehaviour court;

        private AiBrain _brain;
        private IKickable _ball;
        private Vector2 _moveInput;
        private bool _kickPressed;
        private bool _tacklePressed;
        private bool _sprintHeld;
        private float _kickCooldown;

        public Vector2 MoveInput => _moveInput;
        public bool KickPressed => _kickPressed;
        public bool TacklePressed => _tacklePressed;
        public bool SprintHeld => _sprintHeld;

        public void Initialize(AiDifficultyConfig config, IKickable ball, CharacterBehaviour opponentRef, CourtBehaviour courtRef)
        {
            difficultyConfig = config;
            _ball = ball;
            opponent = opponentRef;
            court = courtRef;
            _brain = new AiBrain(config);
        }

        private void Start()
        {
            if (_brain == null && difficultyConfig != null)
            {
                _brain = new AiBrain(difficultyConfig);
            }
        }

        private void Update()
        {
            _kickPressed = false;
            _tacklePressed = false;

            if (_brain == null || _ball == null || opponent == null || court == null) return;

            if (_kickCooldown > 0f) _kickCooldown -= Time.deltaTime;

            Vector3 ownPos = transform.position;
            Vector3 ballPos = _ball.Position;
            Vector3 oppPos = opponent.Position;

            AiGameState state = new AiGameState
            {
                BallPosition = ballPos,
                OwnPosition = ownPos,
                OpponentPosition = oppPos,
                OwnGoalPosition = court.AiGoalCenter,
                OpponentGoalPosition = court.PlayerGoalCenter,
                BallIsPossessedBySelf = _ball.IsPossessed,
                BallIsPossessedByOpponent = false,
                BallIsFree = !_ball.IsPossessed,
                DistanceToBall = Vector3.Distance(ownPos, ballPos),
                OpponentDistanceToBall = Vector3.Distance(oppPos, ballPos),
                DistanceToOwnGoal = Vector3.Distance(ownPos, court.AiGoalCenter),
                DistanceToOpponentGoal = Vector3.Distance(ownPos, court.PlayerGoalCenter),
                DistanceToOpponent = Vector3.Distance(ownPos, oppPos)
            };

            AiAction action = _brain.Decide(state);
            ExecuteAction(action, state);
        }

        private void ExecuteAction(AiAction action, AiGameState state)
        {
            Vector3 targetPos;
            bool sprint = false;

            switch (action)
            {
                case AiAction.ChaseBall:
                    targetPos = state.BallPosition;
                    sprint = state.DistanceToBall > 5f;
                    break;

                case AiAction.DefendGoal:
                    Vector3 defendPoint = state.OwnGoalPosition + (state.BallPosition - state.OwnGoalPosition).normalized * 3f;
                    targetPos = defendPoint;
                    sprint = state.DistanceToOwnGoal > 6f;
                    break;

                case AiAction.AttackWithBall:
                    targetPos = state.OpponentGoalPosition;
                    sprint = state.DistanceToOpponentGoal > 8f;

                    if (state.DistanceToOpponentGoal < 6f && _kickCooldown <= 0f)
                    {
                        _kickPressed = true;
                        _kickCooldown = 0.5f;
                    }
                    break;

                case AiAction.PressOpponent:
                    targetPos = state.OpponentPosition;
                    sprint = state.DistanceToOpponent > 3f;
                    break;

                case AiAction.Tackle:
                    targetPos = state.OpponentPosition;
                    sprint = true;
                    if (state.DistanceToOpponent < 2.5f)
                    {
                        _tacklePressed = true;
                    }
                    break;

                default:
                    targetPos = state.OwnPosition;
                    break;
            }

            Vector3 toTarget = targetPos - state.OwnPosition;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.01f)
            {
                Vector3 dir = toTarget.normalized;
                _moveInput = new Vector2(dir.x, dir.z);
            }
            else
            {
                _moveInput = Vector2.zero;
            }

            _sprintHeld = sprint;
        }

        public void Reset()
        {
            _brain?.Reset();
            _moveInput = Vector2.zero;
            _kickPressed = false;
            _tacklePressed = false;
            _sprintHeld = false;
            _kickCooldown = 0f;
        }
    }
}
