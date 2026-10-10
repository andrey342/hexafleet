using System;
using System.Globalization;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Date a vehicle was manufactured. Holds the date only, as UTC.
    /// </summary>
    public readonly struct ManufacturingDate : IEquatable<ManufacturingDate>
    {
        private readonly DateTime _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManufacturingDate"/> struct.
        /// </summary>
        /// <param name="value">Manufacturing date. The time of day is discarded.</param>
        public ManufacturingDate(DateTime value)
        {
            if (value == DateTime.MinValue)
            {
                throw new DomainException("The manufacturing date is required.");
            }

            _value = DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }

        /// <summary>
        /// Compares two manufacturing dates for equality.
        /// </summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when both dates are the same day.</returns>
        public static bool operator ==(ManufacturingDate left, ManufacturingDate right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two manufacturing dates for inequality.
        /// </summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when the dates are different days.</returns>
        public static bool operator !=(ManufacturingDate left, ManufacturingDate right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets the date value.
        /// </summary>
        /// <returns>The date at midnight UTC.</returns>
        public DateTime ToDateTime()
        {
            return _value;
        }

        /// <inheritdoc />
        public bool Equals(ManufacturingDate other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ManufacturingDate other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
