using UnityEngine;
using StreetLegends.Gameplay.Character;

namespace StreetLegends.Gameplay.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallBehaviour : MonoBehaviour, IKickable
    {
        [SerializeField] private float dragFactor = 0.5f;
        [SerializeField] private float possessionDampening = 8f;
        [SerializeField] private float maxSpeed = 25f;

        private Rigidbody _rb;
        private bool _isPossessed;
        private Transform _owner;

        public Vector3 Position => _rb.position;
        public Vector3 Velocity => _rb.linearVelocity;
        public bool IsPossessed => _isPossessed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (_isPossessed && _owner != null)
            {
                Vector3 targetPos = _owner.position + _owner.forward * 1.2f;
                targetPos.y = _rb.position.y;
                Vector3 direction = (targetPos - _rb.position);
                _rb.linearVelocity = direction * possessionDampening;
            }

            if (_rb.linearVelocity.magnitude > maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
            }

            if (!_isPossessed && _rb.linearVelocity.magnitude > 0.1f)
            {
                _rb.linearVelocity *= (1f - dragFactor * Time.fixedDeltaTime);
            }
        }

        public void Kick(Vector3 direction, float power)
        {
            _isPossessed = false;
            _owner = null;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();
            }
            _rb.linearVelocity = direction * power;
        }

        public void SetPossessed(bool possessed, Transform owner = null)
        {
            _isPossessed = possessed;
            _owner = owner;
        }

        public void Reset(Vector3 position)
        {
            _rb.position = position;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _isPossessed = false;
            _owner = null;
        }
    }
}
