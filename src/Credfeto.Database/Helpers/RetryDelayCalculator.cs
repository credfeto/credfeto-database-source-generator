using System;
using System.Security.Cryptography;

namespace Credfeto.Database.Helpers;

internal static class RetryDelayCalculator
{
    public static TimeSpan Calculate(int attempts)
    {
        // do a fast first retry, then exponential backoff
        return attempts <= 1 ? TimeSpan.Zero : TimeSpan.FromSeconds(CalculateBackoff(attempts));
    }

    public static TimeSpan CalculateWithJitter(int attempts, int maxJitterSeconds)
    {
        // do a fast first retry, then exponential backoff
        return attempts <= 1
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(WithJitter(CalculateBackoff(attempts), maxSeconds: maxJitterSeconds));
    }

    private static double CalculateBackoff(int attempts)
    {
        return Math.Pow(x: 2, y: attempts);
    }

    private static double WithJitter(double delaySeconds, int maxSeconds)
    {
        double nonJitterPeriod = delaySeconds - maxSeconds;
        double jitterRange = maxSeconds * 2;

        if (nonJitterPeriod < 0)
        {
            jitterRange = delaySeconds;
            nonJitterPeriod = delaySeconds / 2;
        }

        double jitter = CalculateJitterSeconds(jitterRange);

        return nonJitterPeriod + jitter;
    }

    private static double CalculateJitterSeconds(double jitterRange)
    {
        return jitterRange * GetRandom();
    }

    private static double GetRandom()
    {
        using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
        {
            Span<byte> rnd = stackalloc byte[sizeof(uint)];
            randomNumberGenerator.GetBytes(rnd);
            uint random = BitConverter.ToUInt32(value: rnd);

            return random / (double)uint.MaxValue;
        }
    }
}
