using System;

namespace GtMotive.Estimate.Microservice.UnitTests.Support
{
    internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private readonly DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
