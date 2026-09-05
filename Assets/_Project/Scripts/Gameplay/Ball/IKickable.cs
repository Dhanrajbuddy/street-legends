using UnityEngine;

namespace StreetLegends.Gameplay.Ball
{
    public interface IKickable
    {
        void Kick(Vector3 direction, float power);
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        bool IsPossessed { get; }
        void SetPossessed(bool possessed, Transform owner = null);
    }
}
