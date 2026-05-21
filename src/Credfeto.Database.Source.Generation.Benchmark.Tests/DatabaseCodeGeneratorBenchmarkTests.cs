using System;
using System.Collections.Generic;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Benchmark.Tests;

public sealed class DatabaseCodeGeneratorBenchmarkTests : TestBase
{
    private const long MaximumAllocatedBytesPerOperation = 24 * 1024;
    private const string GenerateScalarFunctionMethodName = "GenerateScalarFunction";
    private const string GenerateStoredProcedureMethodName = "GenerateStoredProcedure";
    private const string GenerateTableFunctionMethodName = "GenerateTableFunction";

    [Fact]
    public void BenchmarksShouldRemainWithinAllocationBudget()
    {
        (Summary summary, AccumulationLogger logger) = Benchmark<DatabaseCodeGeneratorBenchmarks>();

        Assert.False(summary.HasCriticalValidationErrors, userMessage: logger.GetLog());
        Assert.Equal(expected: 3, actual: summary.Reports.Length);

        Dictionary<string, BenchmarkReport> reports = new(StringComparer.Ordinal)
        {
            [GenerateScalarFunctionMethodName] = GetReport(
                summary: summary,
                benchmarkName: GenerateScalarFunctionMethodName
            ),
            [GenerateTableFunctionMethodName] = GetReport(
                summary: summary,
                benchmarkName: GenerateTableFunctionMethodName
            ),
            [GenerateStoredProcedureMethodName] = GetReport(
                summary: summary,
                benchmarkName: GenerateStoredProcedureMethodName
            ),
        };

        Assert.All(
            reports,
            static benchmark => AssertBenchmarkReport(report: benchmark.Value, benchmarkName: benchmark.Key)
        );
    }

    private static void AssertBenchmarkReport(BenchmarkReport report, string benchmarkName)
    {
        long bytesAllocatedPerOperation = AssertBytesAllocatedPerOperation(report: report, benchmarkName: benchmarkName);

        Assert.True(report.Success, userMessage: $"Benchmark {benchmarkName} did not complete successfully.");
        Assert.True(
            condition: report.GcStats.TotalOperations > 0,
            userMessage: $"Benchmark {benchmarkName} did not record any benchmark operations."
        );
        Assert.Equal(expected: 0, actual: report.GcStats.Gen1Collections);
        Assert.Equal(expected: 0, actual: report.GcStats.Gen2Collections);
        Assert.InRange(
            actual: bytesAllocatedPerOperation,
            low: 1,
            high: MaximumAllocatedBytesPerOperation
        );
    }

    private static long AssertBytesAllocatedPerOperation(BenchmarkReport report, string benchmarkName)
    {
        long? bytesAllocatedPerOperation = report.GcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase);

        Assert.True(
            condition: bytesAllocatedPerOperation.HasValue,
            userMessage: $"Benchmark {benchmarkName} did not report allocated bytes per operation."
        );

        return bytesAllocatedPerOperation.Value;
    }

    private static BenchmarkReport GetReport(Summary summary, string benchmarkName)
    {
        return Assert.Single(
            collection: summary.Reports,
            predicate: report =>
                string.Equals(
                    a: report.BenchmarkCase.Descriptor.WorkloadMethod.Name,
                    b: benchmarkName,
                    comparisonType: StringComparison.Ordinal
                )
        );
    }
}
