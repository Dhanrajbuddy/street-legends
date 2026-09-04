using System;

namespace StreetLegends.Services
{
    /// <summary>
    /// Source of wall-clock time. Abstracted so missions/streaks can be tested and later validated against server time.
    /// </summary>
    public interface ITimeService
    {
        DateTime UtcNow { get; }
    }
}
