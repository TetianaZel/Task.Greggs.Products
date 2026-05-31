using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Greggs.Products.Api;
using Greggs.Products.Api.Exceptions;
using Greggs.Products.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Greggs.Products.UnitTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static HttpContext CreateContext(string path = "/product")
    {
        var ctx = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };
        ctx.Request.Path = path;
        ctx.Request.Method = HttpMethods.Get;
        return ctx;
    }

    private static ExceptionHandlingMiddleware CreateSut(RequestDelegate next)
    {
        return new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
    }

    private static async Task<ProblemDetails> ReadProblemAsync(HttpContext ctx)
    {
        ctx.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(ctx.Response.Body, JsonOptions);
        Assert.NotNull(problem);
        return problem!;
    }

    [Fact]
    public async Task InvokeAsync_NoException_PassesThroughUntouched()
    {
        var ctx = CreateContext();
        var nextCalled = false;

        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await CreateSut(next).InvokeAsync(ctx);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, ctx.Response.StatusCode);
        Assert.Equal(0, ctx.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400ProblemDetails()
    {
        var ctx = CreateContext("/product");
        RequestDelegate next = _ => throw new ValidationException("Currency 'ZZZ' is not supported.");

        await CreateSut(next).InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);
        Assert.Equal(Constants.Defaults.ProblemContentType, ctx.Response.ContentType);

        var problem = await ReadProblemAsync(ctx);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(Constants.ErrorMessages.InvalidRequestTitle, problem.Title);
        Assert.Equal("Currency 'ZZZ' is not supported.", problem.Detail);
        Assert.Equal("/product", problem.Instance);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500ProblemDetails_WithoutLeakingMessage()
    {
        var ctx = CreateContext();
        RequestDelegate next = _ => throw new InvalidOperationException("internal database password leaked");

        await CreateSut(next).InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
        Assert.Equal(Constants.Defaults.ProblemContentType, ctx.Response.ContentType);

        var problem = await ReadProblemAsync(ctx);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal(Constants.ErrorMessages.UnexpectedErrorTitle, problem.Title);
        Assert.Null(problem.Detail); // sensitive details must NOT be exposed to the client
    }
}