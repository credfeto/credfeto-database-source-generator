using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Credfeto.Database.SqlServer.Extensions;

internal static class SqlErrorBuilder
{
    public static StringBuilder AppendErrorsFromException(this StringBuilder sb, SqlException sqlException, ref int initialError)
    {
        int error = initialError;

        try
        {
            return sqlException.Errors.OfType<SqlError>()
                               .Aggregate(seed: sb, func: AppendError);

            StringBuilder AppendError(StringBuilder stringBuilder, SqlError sqlError)
            {
                return stringBuilder.Append(++error)
                                    .Append(": Error ")
                                    .Append(sqlError.Number)
                                    .Append(". Proc: ")
                                    .Append(sqlError.Procedure)
                                    .Append(": ")
                                    .AppendLine(sqlError.Message);
            }
        }
        finally
        {
            initialError = error;
        }
    }
}