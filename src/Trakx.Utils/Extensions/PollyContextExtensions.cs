using Microsoft.Extensions.Logging;
using Polly;

namespace Trakx.Utils.Extensions
{
    public static class PollyContextExtensions
    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static bool TryGetLogger(this Context context, out ILogger? logger)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if (context.TryGetValue("Logger", out var loggerObject) && loggerObject is ILogger contextLogger)
            {
                logger = contextLogger;
                return true;
            }

            logger = null;
            return false;
        }
    }
}
