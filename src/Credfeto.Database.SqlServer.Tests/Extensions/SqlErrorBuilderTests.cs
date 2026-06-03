using System.Text;
using Credfeto.Database.SqlServer.Extensions;
using Credfeto.Database.SqlServer.Tests.Helpers;
using FunFair.Test.Common;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Credfeto.Database.SqlServer.Tests.Extensions;

public sealed class SqlErrorBuilderTests : TestBase
{
    [Fact]
    public void AppendErrorsFromException_WithNoErrors_ReturnsSameStringBuilder()
    {
        SqlException sqlException = SqlExceptionFactory.CreateSqlException();
        StringBuilder sb = new();
        int initialError = 0;

        StringBuilder result = sb.AppendErrorsFromException(sqlException: sqlException, initialError: ref initialError);

        Assert.Same(expected: sb, actual: result);
        Assert.Equal(expected: string.Empty, actual: result.ToString());
        Assert.Equal(expected: 0, actual: initialError);
    }

    [Fact]
    public void AppendErrorsFromException_WithOneError_AppendsErrorToStringBuilder()
    {
        SqlError error = SqlExceptionFactory.CreateSqlError(
            errorNumber: 1234,
            message: "Test error message",
            procedure: "usp_TestProc"
        );
        SqlException sqlException = SqlExceptionFactory.CreateSqlException(error);
        StringBuilder sb = new();
        int initialError = 0;

        sb.AppendErrorsFromException(sqlException: sqlException, initialError: ref initialError);

        string result = sb.ToString();
        Assert.Contains(
            expectedSubstring: "1234",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "usp_TestProc",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "Test error message",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Equal(expected: 1, actual: initialError);
    }

    [Fact]
    public void AppendErrorsFromException_WithMultipleErrors_AppendsAllErrors()
    {
        SqlError error1 = SqlExceptionFactory.CreateSqlError(
            errorNumber: 100,
            message: "First error",
            procedure: "usp_Proc1"
        );
        SqlError error2 = SqlExceptionFactory.CreateSqlError(
            errorNumber: 200,
            message: "Second error",
            procedure: "usp_Proc2"
        );
        SqlException sqlException = SqlExceptionFactory.CreateSqlException(error1, error2);

        StringBuilder sb = new();
        int initialError = 0;

        sb.AppendErrorsFromException(sqlException: sqlException, initialError: ref initialError);

        string result = sb.ToString();
        Assert.Contains(
            expectedSubstring: "100",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "First error",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "200",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "Second error",
            actualString: result,
            comparisonType: System.StringComparison.Ordinal
        );
        Assert.Equal(expected: 2, actual: initialError);
    }

    [Fact]
    public void AppendErrorsFromException_WithInitialErrorNonZero_ContinuesCountingFromInitialError()
    {
        SqlError error = SqlExceptionFactory.CreateSqlError(errorNumber: 999, message: "Test", procedure: "usp_Test");
        SqlException sqlException = SqlExceptionFactory.CreateSqlException(error);
        StringBuilder sb = new();
        int initialError = 5;

        sb.AppendErrorsFromException(sqlException: sqlException, initialError: ref initialError);

        Assert.Equal(expected: 6, actual: initialError);
    }
}
