using System;

namespace Credfeto.Database.Helpers;

internal static class RetryDelayCalculator
{
    public static TimeSpan Calculate(int attempts)
    {
        // do a fast first retry, then exponential backoff
        return attempts <= 1 ? TimeSpan.Zero : TimeSpan.FromSeconds(CalculateBackoff(attempts));
    }

    private static double CalculateBackoff(int attempts)
    {
        return Math.Pow(x: 2, y: attempts);
    }
}
