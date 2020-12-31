using System;

namespace Trakx.Utils.DateTimeHelpers
{

    public readonly struct TimeInterval : IEquatable<TimeInterval>
    {
        #region Equality members

        /// <inheritdoc />
        public bool Equals(TimeInterval other)
        {
            return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TimeInterval other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (StartTime.GetHashCode() * 397) ^ EndTime.GetHashCode();
            }
        }

        public static bool operator ==(TimeInterval left, TimeInterval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeInterval left, TimeInterval right)
        {
            return !left.Equals(right);
        }

        #endregion

        public TimeInterval(System.DateTime startTime, System.DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        /// Inclusive lower bound of the interval.
        /// </summary>
        public System.DateTime StartTime { get; }

        /// <summary>
        /// Exclusive upper bound of the interval.
        /// </summary>
        public System.DateTime EndTime { get; }
    }
}
