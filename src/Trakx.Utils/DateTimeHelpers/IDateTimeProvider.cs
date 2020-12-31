using System;

namespace Trakx.Utils.DateTimeHelpers
{
    /// <summary>
    /// Allows easier testing, by setting fixed return values.
    /// </summary>
    public interface IDateTimeProvider
    {
        System.DateTime UtcNow { get; }
        DateTimeOffset UtcNowAsOffset { get; }
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        #region Implementation of IDateTimeProvider

        /// <inheritdoc />
        public System.DateTime UtcNow => System.DateTime.UtcNow;
        public DateTimeOffset UtcNowAsOffset => DateTimeOffset.UtcNow;
        #endregion
    }
}