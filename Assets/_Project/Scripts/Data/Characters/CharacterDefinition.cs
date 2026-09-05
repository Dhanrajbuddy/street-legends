using UnityEngine;

namespace StreetLegends.Data.Characters
{
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Street Legends/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        public string displayName = "Rookie";
        [Min(1f)] public float moveSpeed = 6f;
        [Min(1f)] public float sprintSpeed = 9f;
        [Min(1f)] public float kickPower = 12f;
        [Min(0.5f)] public float kickRange = 1.5f;
        [Min(0.5f)] public float ballControlRange = 2f;
        [Min(0.5f)] public float tackleRange = 1.8f;
        [Min(0.1f)] public float tackleCooldown = 2f;
        [Min(0.1f)] public float tackleDuration = 0.4f;
        [Min(0f)] public float rotationSpeed = 720f;
        [Min(0.1f)] public float acceleration = 30f;
        [Min(0.1f)] public float deceleration = 40f;
    }
}
