using System;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>
    /// Vehicle identifier.
    /// </summary>
    public readonly struct VehicleId : IEquatable<VehicleId>
    {
        private readonly Guid _value;

        private VehicleId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new DomainException("The vehicle id is required.");
            }

            _value = value;
        }

        /// <summary>
        /// Compares two identifiers for equality.
        /// </summary>
        /// <param name="left">Left identifier.</param>
        /// <param name="right">Right identifier.</param>
        /// <returns>True when both identifiers have the same value.</returns>
        public static bool operator ==(VehicleId left, VehicleId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two identifiers for inequality.
        /// </summary>
        /// <param name="left">Left identifier.</param>
        /// <param name="right">Right identifier.</param>
        /// <returns>True when the identifiers have different values.</returns>
        public static bool operator !=(VehicleId left, VehicleId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Creates a new identifier.
        /// </summary>
        /// <returns>A new vehicle identifier.</returns>
        public static VehicleId New()
        {
            return new VehicleId(Guid.NewGuid());
        }

        /// <summary>
        /// Creates an identifier from an existing value.
        /// </summary>
        /// <param name="value">Identifier value.</param>
        /// <returns>The vehicle identifier.</returns>
        public static VehicleId From(Guid value)
        {
            return new VehicleId(value);
        }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <returns>The identifier as a <see cref="Guid"/>.</returns>
        public Guid ToGuid()
        {
            return _value;
        }

        /// <inheritdoc />
        public bool Equals(VehicleId other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is VehicleId other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
