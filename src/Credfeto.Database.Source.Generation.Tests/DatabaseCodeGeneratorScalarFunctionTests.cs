using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Tests.Helpers;
using Credfeto.Database.Source.Generation.Tests.Verifiers;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.Source.Generation.Tests;

[SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
public sealed class DatabaseCodeGeneratorScalarFunctionTests : GeneratorVerifierTestsBase<DatabaseCodeGenerator>
{
    public DatabaseCodeGeneratorScalarFunctionTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public Task SimpleScalarFunctionIntAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

#nullable enable

" + Constants.DatabaseTypes + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<int> GetValueAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<int> GetValueAsync(System.Data.Common.DbConnection connection, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction()"";

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(""No result returned."");
        }

        return Convert.ToInt32(result);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionIntWithOneParameterAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

#nullable enable

" + Constants.DatabaseTypes + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<int> GetValueAsync(DbConnection connection, int factor, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<int> GetValueAsync(System.Data.Common.DbConnection connection, int factor, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction(@factor)"";
        DbParameter p0 = command.CreateParameter();
        p0.Value = factor;
        p0.DbType = DbType.Int32;
        p0.ParameterName = ""@factor"";
        command.Parameters.Add(p0);

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(""No result returned."");
        }

        return Convert.ToInt32(result);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionIntWithTwoParametersAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

#nullable enable

" + Constants.DatabaseTypes + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<int> GetValueAsync(DbConnection connection, int factor, string name, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<int> GetValueAsync(System.Data.Common.DbConnection connection, int factor, string name, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction(@factor, @name)"";
        DbParameter p0 = command.CreateParameter();
        p0.Value = factor;
        p0.DbType = DbType.Int32;
        p0.ParameterName = ""@factor"";
        command.Parameters.Add(p0);
        DbParameter p1 = command.CreateParameter();
        p1.Value = name;
        p1.DbType = DbType.String;
        p1.ParameterName = ""@name"";
        command.Parameters.Add(p1);

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(""No result returned."");
        }

        return Convert.ToInt32(result);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionIntWithThreeParametersAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Mappers;
using Primatives;

#nullable enable

" + Constants.DatabaseTypes + Constants.AccountAddressClass + Constants.AccountAddressMapperClass + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<int> GetValueAsync(
                DbConnection connection,
                int factor,
                string name,
                [SqlFieldMap<AccountAddressMapper, AccountAddress>]
                AccountAddress address,
                CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<int> GetValueAsync(System.Data.Common.DbConnection connection, int factor, string name, Primatives.AccountAddress address, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction(@factor, @name, @address)"";
        DbParameter p0 = command.CreateParameter();
        p0.Value = factor;
        p0.DbType = DbType.Int32;
        p0.ParameterName = ""@factor"";
        command.Parameters.Add(p0);
        DbParameter p1 = command.CreateParameter();
        p1.Value = name;
        p1.DbType = DbType.String;
        p1.ParameterName = ""@name"";
        command.Parameters.Add(p1);
        DbParameter p2 = command.CreateParameter();
        Mappers.AccountAddressMapper.MapToDb(address, p2);
        p2.ParameterName = ""@address"";
        command.Parameters.Add(p2);

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(""No result returned."");
        }

        return Convert.ToInt32(result);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionStringAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

#nullable enable

" + Constants.DatabaseTypes + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<string> GetValueAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<string> GetValueAsync(System.Data.Common.DbConnection connection, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction()"";

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(""No result returned."");
        }

        return (string)result;
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionAccountAddressAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Mappers;
using Primatives;

#nullable enable

" + Constants.DatabaseTypes + Constants.AccountAddressClass + Constants.AccountAddressMapperClass + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        [return: SqlFieldMap<AccountAddressMapper, AccountAddress>]
        public static partial Task<AccountAddress> GetValueAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<Primatives.AccountAddress> GetValueAsync(System.Data.Common.DbConnection connection, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction()"";

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(""No result returned."");
        }

        return Mappers.AccountAddressMapper.MapFromDb(value: result);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionNullableIntAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

#nullable enable

" + Constants.DatabaseTypes + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<int?> GetValueAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<int?> GetValueAsync(System.Data.Common.DbConnection connection, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction()"";

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            return null;
        }

        return Convert.ToInt32(result);
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }

    [Fact]
    public Task SimpleScalarFunctionNullableStringAsync()
    {
        const string test = @"
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

#nullable enable

" + Constants.DatabaseTypes + @"

namespace ConsoleApplication1
{
    public static partial class DatabaseWrapper
    {
        [SqlObjectMap(name: ""example.scalarfunction"", sqlObjectType: SqlObjectType.SCALAR_FUNCTION)]
        public static partial Task<string?> GetValueAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}";

        (string filename, string generated)[] expected =
        {
            (filename: "ConsoleApplication1.DatabaseWrapper.GetValueAsync.Database.generated.cs", generated: @"using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace ConsoleApplication1;

public static partial class DatabaseWrapper
{
    [GeneratedCode(tool: ""Credfeto.Database.Source.Generation.DatabaseCodeGenerator"", version: """ + VersionInformation.Version() + @""")]
    public static async partial System.Threading.Tasks.Task<string?> GetValueAsync(System.Data.Common.DbConnection connection, System.Threading.CancellationToken cancellationToken)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = ""select example.scalarfunction()"";

        object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            return null;
        }

        return (string)result;
    }
}
")
        };

        return this.VerifyAsync(code: test, expected: expected, cancellationToken: CancellationToken.None);
    }
}