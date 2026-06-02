using System;
using Credfeto.Database.Source.Generation.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests.Exceptions;

public sealed class InvalidModelExceptionTests : TestBase
{
    [Fact]
    public void DefaultConstructorUsesDefaultMessage()
    {
        InvalidModelException exception = new();
        Assert.Equal(expected: "Invalid model", actual: exception.Message);
    }

    [Fact]
    public void MessageConstructorUsesProvidedMessage()
    {
        const string expectedMessage = "Custom error message";
        InvalidModelException exception = new(expectedMessage);
        Assert.Equal(expected: expectedMessage, actual: exception.Message);
    }

    [Fact]
    public void InnerExceptionConstructorSetsInnerException()
    {
        const string expectedMessage = "Outer message";
        InvalidOperationException innerException = new("inner");
        InvalidModelException exception = new(message: expectedMessage, innerException: innerException);
        Assert.Equal(expected: expectedMessage, actual: exception.Message);
        Assert.Same(expected: innerException, actual: exception.InnerException);
    }

    [Fact]
    public void IsDerivedException()
    {
        InvalidModelException exception = new();
        Assert.IsAssignableFrom<Exception>(exception);
    }
}
