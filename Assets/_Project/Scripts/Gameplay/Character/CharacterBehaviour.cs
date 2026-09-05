using UnityEngine;
using StreetLegends.Data.Characters;
using StreetLegends.Gameplay.Input;
using StreetLegends.Gameplay.Ball;
using StreetLegends.Gameplay.Court;

namespace StreetLegends.Gameplay.Character
{
    [RequireComponent(typeof(Collider))]
    public class CharacterBehaviour : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition definition;
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        private CharacterRuntimeStats _stats;
        private CharacterMotor _motor;
        private CharacterActions _actions;
        private IInputSource _input;
        private IKickable _ball;
        private CourtBehaviour _court;
        private CharacterBehaviour _opponent;
        private Transform _transform;

        public CharacterRuntimeStats Stats => _stats;
        public CharacterMotor Motor => _motor;
        public CharacterActions Actions => _actions;
        public Vector3 Position => _transform.position;
        public bool HasBall => _ball != null && _ball.IsPossessed && _ball.IsPossessed;

        public void Initialize(CharacterDefinition def, IInputSource input, IKickable ball,
            CourtBehaviour court, CharacterBehaviour opponent, Vector3 startPosition)
        {
            definition = def;
            _stats = new CharacterRuntimeStats(def);
            _motor = new CharacterMotor(_stats, startPosition);
            _actions = new CharacterActions(_stats);
            _input = input;
            _ball = ball;
            _court = court;
            _opponent = opponent;
            _transform.position = startPosition;
        }

        public void SetBall(IKickable ball) => _ball = ball;
        public void SetOpponent(CharacterBehaviour opponent) => _opponent = opponent;

        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            if (definition != null && _stats == null)
            {
                _stats = new CharacterRuntimeStats(definition);
                _motor = new CharacterMotor(_stats, _transform.position);
                _actions = new CharacterActions(_stats);
            }

            if (inputSourceBehaviour is IInputSource source)
            {
                _input = source;
            }
        }

        private void Update()
        {
            if (_stats == null || _motor == null || _actions == null || _input == null) return;

            _actions.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_stats == null || _motor == null || _actions == null || _input == null) return;

            Bounds bounds = _court != null ? _court.PlayBounds : new Bounds(Vector3.zero, new Vector3(100f, 0f, 100f));
            _motor.Update(_input.MoveInput, _input.SprintHeld, Time.fixedDeltaTime, bounds);

            _transform.position = _motor.Position;
            _transform.rotation = _motor.Rotation;

            HandleBallInteraction();
            HandleKick();
            HandleTackle();
        }

        private void HandleBallInteraction()
        {
            if (_ball == null) return;

            float distToBall = Vector3.Distance(_transform.position, _ball.Position);

            if (!_ball.IsPossessed && distToBall < _stats.BallControlRange)
            {
                Vector3 toBall = (_ball.Position - _transform.position).normalized;
                float dot = Vector3.Dot(_transform.forward, toBall);
                if (dot > 0.3f && _ball.Velocity.magnitude < 8f)
                {
                    _ball.SetPossessed(true, _transform);
                }
            }
        }

        private void HandleKick()
        {
            if (_ball == null || !_input.KickPressed) return;

            float distToBall = Vector3.Distance(_transform.position, _ball.Position);
            if (distToBall > _stats.KickRange + _stats.BallControlRange) return;

            Vector3 kickDir = _transform.forward;
            if (_court != null)
            {
                Vector3 opponentGoal = _court.AiGoalCenter;
                if (_opponent != null && _opponent == this)
                {
                    opponentGoal = _court.PlayerGoalCenter;
                }
                Vector3 toGoal = (opponentGoal - _ball.Position).normalized;
                kickDir = Vector3.Lerp(kickDir, toGoal, 0.5f).normalized;
            }

            _ball.Kick(kickDir, _stats.KickPower);
        }

        private void HandleTackle()
        {
            if (_opponent == null || !_input.TacklePressed) return;
            if (!_actions.TryTackle()) return;

            float distToOpponent = Vector3.Distance(_transform.position, _opponent.Position);
            if (distToOpponent <= _stats.TackleRange)
            {
                if (_ball != null && _ball.IsPossessed)
                {
                    _ball.SetPossessed(false);
                    _ball.Kick(_transform.forward, _stats.KickPower * 0.5f);
                }
            }
        }

        public void ResetPosition(Vector3 position)
        {
            _motor?.Reset(position);
            _actions?.Reset();
            _transform.position = position;
            _transform.rotation = Quaternion.identity;
        }
    }
}
