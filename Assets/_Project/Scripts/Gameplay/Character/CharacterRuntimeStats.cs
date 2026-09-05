using StreetLegends.Data.Characters;

namespace StreetLegends.Gameplay.Character
{
    public class CharacterRuntimeStats
    {
        public float MoveSpeed { get; }
        public float SprintSpeed { get; }
        public float KickPower { get; }
        public float KickRange { get; }
        public float BallControlRange { get; }
        public float TackleRange { get; }
        public float TackleCooldown { get; }
        public float TackleDuration { get; }
        public float RotationSpeed { get; }
        public float Acceleration { get; }
        public float Deceleration { get; }

        public CharacterRuntimeStats(CharacterDefinition def)
        {
            MoveSpeed = def.moveSpeed;
            SprintSpeed = def.sprintSpeed;
            KickPower = def.kickPower;
            KickRange = def.kickRange;
            BallControlRange = def.ballControlRange;
            TackleRange = def.tackleRange;
            TackleCooldown = def.tackleCooldown;
            TackleDuration = def.tackleDuration;
            RotationSpeed = def.rotationSpeed;
            Acceleration = def.acceleration;
            Deceleration = def.deceleration;
        }
    }
}
