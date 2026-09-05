using UnityEngine;

namespace StreetLegends.Gameplay.Character
{
    public class CharacterMotor
    {
        private readonly CharacterRuntimeStats _stats;
        private Vector3 _velocity;
        private Vector3 _position;
        private Quaternion _rotation;

        public Vector3 Position => _position;
        public Vector3 Velocity => _velocity;
        public Quaternion Rotation => _rotation;
        public bool IsMoving => _velocity.sqrMagnitude > 0.01f;

        public CharacterMotor(CharacterRuntimeStats stats, Vector3 startPosition)
        {
            _stats = stats;
            _position = startPosition;
            _rotation = Quaternion.identity;
            _velocity = Vector3.zero;
        }

        public void Update(Vector2 moveInput, bool sprint, float deltaTime, Bounds courtBounds)
        {
            float targetSpeed = sprint ? _stats.SprintSpeed : _stats.MoveSpeed;
            Vector3 desiredVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * targetSpeed;

            float accelRate = desiredVelocity.sqrMagnitude > 0.1f ? _stats.Acceleration : _stats.Deceleration;
            _velocity = Vector3.MoveTowards(_velocity, desiredVelocity, accelRate * deltaTime);

            _position += _velocity * deltaTime;
            _position = ClampToBounds(_position, courtBounds);

            if (_velocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_velocity.normalized);
                _rotation = Quaternion.RotateTowards(_rotation, targetRot, _stats.RotationSpeed * deltaTime);
            }
        }

        public void Reset(Vector3 position)
        {
            _position = position;
            _velocity = Vector3.zero;
            _rotation = Quaternion.identity;
        }

        public void SetPosition(Vector3 position)
        {
            _position = position;
        }

        private static Vector3 ClampToBounds(Vector3 pos, Bounds bounds)
        {
            pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
            pos.z = Mathf.Clamp(pos.z, bounds.min.z, bounds.max.z);
            pos.y = 0f;
            return pos;
        }
    }
}
