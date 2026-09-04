using System;
using StreetLegends.Services;

namespace StreetLegends.Services.Local
{
    public sealed class SystemTimeService : ITimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
