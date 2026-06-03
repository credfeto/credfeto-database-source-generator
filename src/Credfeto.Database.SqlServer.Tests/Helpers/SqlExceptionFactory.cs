using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Credfeto.Database.SqlServer.Tests.Helpers;

internal static class SqlExceptionFactory
{
    [UnconditionalSuppressMessage(
        category: "Trimming",
        checkId: "IL2026:RequiresUnreferencedCode",
        Justification = "Test code using reflection against internal types"
    )]
    [UnconditionalSuppressMessage(
        category: "Trimming",
        checkId: "IL2075:DynamicallyAccessedMembersRequirement",
        Justification = "Test code using reflection against internal types"
    )]
    public static SqlError CreateSqlError(
        int errorNumber,
        string message = "test error",
        string procedure = "usp_TestProc"
    )
    {
        Assembly assembly = typeof(SqlException).Assembly;
        Type sqlErrorType =
            assembly.GetType("Microsoft.Data.SqlClient.SqlError")
            ?? throw new InvalidOperationException("Cannot find SqlError type");

        ConstructorInfo ctor =
            sqlErrorType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types:
                [
                    typeof(int),
                    typeof(byte),
                    typeof(byte),
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(int),
                    typeof(Exception),
                ],
                modifiers: null
            ) ?? throw new InvalidOperationException("Cannot find SqlError constructor");

        return (SqlError)(
            ctor.Invoke([errorNumber, (byte)0, (byte)0, "server", message, procedure, 0, null])
            ?? throw new InvalidOperationException("SqlError constructor returned null")
        );
    }

    [UnconditionalSuppressMessage(
        category: "Trimming",
        checkId: "IL2026:RequiresUnreferencedCode",
        Justification = "Test code using reflection against internal types"
    )]
    [UnconditionalSuppressMessage(
        category: "Trimming",
        checkId: "IL2075:DynamicallyAccessedMembersRequirement",
        Justification = "Test code using reflection against internal types"
    )]
    public static SqlException CreateSqlException(params SqlError[] errors)
    {
        Assembly assembly = typeof(SqlException).Assembly;

        Type sqlErrorCollectionType =
            assembly.GetType("Microsoft.Data.SqlClient.SqlErrorCollection")
            ?? throw new InvalidOperationException("Cannot find SqlErrorCollection type");

        ConstructorInfo collectionCtor =
            sqlErrorCollectionType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null
            ) ?? throw new InvalidOperationException("Cannot find SqlErrorCollection constructor");

        object collection =
            collectionCtor.Invoke([])
            ?? throw new InvalidOperationException("SqlErrorCollection constructor returned null");

        MethodInfo addMethod =
            sqlErrorCollectionType.GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Cannot find SqlErrorCollection.Add method");

        foreach (SqlError error in errors)
        {
            addMethod.Invoke(obj: collection, parameters: [error]);
        }

        MethodInfo createException =
            typeof(SqlException).GetMethod(
                "CreateException",
                BindingFlags.Static | BindingFlags.NonPublic,
                binder: null,
                types: [sqlErrorCollectionType, typeof(string)],
                modifiers: null
            ) ?? throw new InvalidOperationException("Cannot find SqlException.CreateException method");

        return (SqlException)(
            createException.Invoke(obj: null, parameters: [collection, "7.0"])
            ?? throw new InvalidOperationException("CreateException returned null")
        );
    }
}
