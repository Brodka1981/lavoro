using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Various;
public class ApiCallOutputTests : TestBase
{
    public ApiCallOutputTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Unit]
    public async Task ApiCallOutput_Success()
    {
        var apiCallOutput = new ApiCallOutput()
        {
            Success = true,
        };
        apiCallOutput.ToString().Should().Be("Ok");
    }

    [Fact]
    [Unit]
    public async Task ApiCallOutput_InternalServerError()
    {
        var apiCallOutput = new ApiCallOutput()
        {
            InternalServerError = "Error"
        };
        apiCallOutput.ToString().Should().Be("KO: Error");
    }

    [Fact]
    [Unit]
    public async Task ApiCallOutput_Error()
    {
        var apiCallOutput = new ApiCallOutput()
        {
            Err = ApiError.New(HttpStatusCode.BadRequest, "code", "message", "endpoint")
        };
        apiCallOutput.ToString().Should().Be("KO: code - message");
    }

    [Fact]
    [Unit]
    public async Task ApiCallOutput_KO()
    {
        var apiCallOutput = new ApiCallOutput();
        apiCallOutput.ToString().Should().Be("KO");
    }

    [Fact]
    [Unit]
    public async Task ProblemDetail()
    {
        var problemDetailError = new ProblemDetailError("a", "b");
        problemDetailError.Field.Should().Be("a");
        problemDetailError.ErrorMessage.Should().Be("b");
    }

    [Fact]
    [Unit]
    public async Task ApiCallOutput_Clone_Success()
    {
        var sourceApiCallOutput = new ApiCallOutput<long>()
        {
            Success = true,
            Err = new ApiError() { Code = "code" },
            InternalServerError = "InternalServerError ",
            Result = 2,
            StatusCode = HttpStatusCode.OK
        };
        var targetApiCallOutput = new ApiCallOutput<bool>().Clone(sourceApiCallOutput);

        targetApiCallOutput.Success.Should().Be(sourceApiCallOutput.Success);
        targetApiCallOutput.Err.Should().NotBeNull();
        targetApiCallOutput.Err.Code.Should().Be(sourceApiCallOutput.Err.Code);
        targetApiCallOutput.InternalServerError.Should().Be(sourceApiCallOutput.InternalServerError);
        targetApiCallOutput.StatusCode.Should().Be(sourceApiCallOutput.StatusCode);
    }

    [Fact]
    [Unit]
    public async Task ApiError_New()
    {
        var apiError = ApiError.New(HttpStatusCode.BadRequest, "code", "message", "endpoint");
        apiError.Should().NotBeNull();
        apiError.Status.Should().Be((int)HttpStatusCode.BadRequest);
        apiError.Type.Should().Be("endpoint");
        apiError.Title.Should().Be("Api Call Error");
        apiError.Detail.Should().Be("message");
        apiError.Code.Should().Be("code");
    }

    [Fact]
    [Unit]
    public async Task ApiError_Tostring()
    {
        var apiError = ApiError.New(HttpStatusCode.BadRequest, "code", "message", "endpoint");
        apiError.Should().NotBeNull();
        apiError.ToString().Should().Be("code - message");
    }
}
