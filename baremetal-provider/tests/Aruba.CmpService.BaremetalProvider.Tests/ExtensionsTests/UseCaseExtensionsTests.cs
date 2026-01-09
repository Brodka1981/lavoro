using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Tests.Extensions;
using Aruba.MessageBus.Models;
using Aruba.MessageBus.UseCases;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class UseCaseExtensionsTests :
    TestBase
{
    public UseCaseExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_NotFound()
    {
        var result = ServiceResult<object>.CreateNotFound(Guid.NewGuid());
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.NotFound);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_InternalServerError_NoOutbox()
    {
        var result = ServiceResult<object>.CreateInternalServerError();
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Failure);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_InternalServerError_Outbox_Null()
    {
        var result = ServiceResult<object>.CreateInternalServerError();
        var ret = new TestTypedUseCase(result).ExecutedFailure(new ArgumentOutOfRangeException(), null);
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Failure);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_InternalServerError_With_Outbox()
    {
        var result = ServiceResult<object>.CreateInternalServerError();
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Failure);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_Forbidden()
    {
        var result = ServiceResult<object>.CreateForbiddenError();
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Forbidden);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_Success_NoOutbox()
    {
        var result = new ServiceResult<object>() { Value = 1 };
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Success);
        messageType.Value.Should().Be(1);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_Success_WithOutbox()
    {
        var result = new ServiceResult<object>() { Value = 1 };
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Success);
        messageType.Value.Should().Be(1);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_BadRequest_NoOutbox()
    {
        var result = ServiceResult<object>.CreateBadRequestError(FieldNames.Name, "InvalidName");
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(1);
        messageType.Errors.First().Should().BeOfType<BadRequestError>();
        messageType.Errors.First().FieldName.Should().Be(FieldNames.Name.Value);
        messageType.Errors.First().ErrorMessage.Should().Be("InvalidName");
        messageType.Errors.First().Params.Should().HaveCount(0);
        messageType.Errors.First().ErrorLabel.Should().BeNull();
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_BadRequest_WithOutBox()
    {
        var result = ServiceResult<object>.CreateBadRequestError(FieldNames.Name, "InvalidName");
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(1);
        messageType.Errors.First().Should().BeOfType<BadRequestError>();
        messageType.Errors.First().FieldName.Should().Be(FieldNames.Name.Value);
        messageType.Errors.First().ErrorMessage.Should().Be("InvalidName");
        messageType.Errors.First().Params.Should().HaveCount(0);
        messageType.Errors.First().ErrorLabel.Should().BeNull();
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().NotBeNull();
    }


    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_LegacyBadRequest_NoOutbox()
    {
        var result = ServiceResult<object>.CreateBadRequestError(FieldNames.Name, LegacyLabelErrors.Create("ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH", Typologies.Server)!);
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(1);
        messageType.Errors.First().Should().BeOfType<BadRequestError>();
        messageType.Errors.First().FieldName.Should().Be(FieldNames.Name.Value);
        messageType.Errors.First().ErrorMessage.Should().Be("The resource name length must be be between 4 and 50 characters");
        messageType.Errors.First().Params.Should().HaveCount(0);
        messageType.Errors.First().ErrorLabel.Should().NotBeNull();
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }


    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_LegacyBadRequest_OutboxNull()
    {
        var result = ServiceResult<object>.CreateBadRequestError(FieldNames.Name, LegacyLabelErrors.Create("ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH", Typologies.Server)!);
        var ret = new TestTypedUseCase(result).ExecutedValidation(null);
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(0);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Typed_LegacyBadRequest_WithOutBox()
    {
        var result = ServiceResult<object>.CreateBadRequestError(FieldNames.Name, LegacyLabelErrors.Create("ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH", Typologies.Server)!);
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestTypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(1);
        messageType.Errors.First().Should().BeOfType<BadRequestError>();
        messageType.Errors.First().FieldName.Should().Be(FieldNames.Name.Value);
        messageType.Errors.First().ErrorMessage.Should().Be("The resource name length must be be between 4 and 50 characters");
        messageType.Errors.First().Params.Should().HaveCount(0);
        messageType.Errors.First().ErrorLabel.Should().NotBeNull();
        var messageOutbox = ret.GetOutboxMessageType<Outbox, Response>();
        messageOutbox.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_NotFound()
    {
        var result = new ServiceResult() { Errors = new List<IServiceResultError>() { new NotFoundError(Guid.NewGuid()) } };
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.NotFound);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_InternalServerError_NoOutbox()
    {
        var result = new ServiceResult() { Errors = new List<IServiceResultError>() { new FailureError() } };
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Failure);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_InternalServerError_WithOutbox()
    {
        var result = new ServiceResult() { Errors = new List<IServiceResultError>() { new FailureError() } };
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Failure);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_Forbidden()
    {
        var result = new ServiceResult() { Errors = new List<IServiceResultError>() { new ForbiddenError() } };
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Forbidden);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().BeNull();

    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_Success_No_Outbox()
    {
        var result = new ServiceResult();
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Success);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_Success_With_Outbox()
    {
        var result = new ServiceResult();
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Success);
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_BadRequest_No_Outbox()
    {
        var result = new ServiceResult() { Errors = new List<IServiceResultError>() { BadRequestError.Create(FieldNames.Name, "InvalidName") } };
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(1);
        messageType.Errors.First().Should().BeOfType<BadRequestError>();
        messageType.Errors.First().FieldName.Should().Be(FieldNames.Name.Value);
        messageType.Errors.First().ErrorMessage.Should().Be("InvalidName");
        messageType.Errors.First().Params.Should().HaveCount(0);
        messageType.Errors.First().ErrorLabel.Should().BeNull();
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ExecutedResult_Untyped_BadRequest_With_Outbox()
    {
        var result = new ServiceResult() { Errors = new List<IServiceResultError>() { BadRequestError.Create(FieldNames.Name, "InvalidName") } };
        result.AddEnvelopes(new EnvelopeBuilder().WithSubject("a").Build(new Outbox()));
        var ret = await new TestUntypedUseCase(result).Execute();
        var messageType = ret.GetResponseMessageType();
        messageType.Type.Should().Be(MessageBusResponseTypes.Validation);
        messageType.Errors.Should().HaveCount(1);
        messageType.Errors.First().Should().BeOfType<BadRequestError>();
        messageType.Errors.First().FieldName.Should().Be(FieldNames.Name.Value);
        messageType.Errors.First().ErrorMessage.Should().Be("InvalidName");
        messageType.Errors.First().Params.Should().HaveCount(0);
        messageType.Errors.First().ErrorLabel.Should().BeNull();
        var messageOutbox = ret.GetOutboxMessageType<Outbox, EmptyResponse>();
        messageOutbox.Should().NotBeNull();
    }
}

internal class TestTypedUseCase : UseCase<Request, Response>
{
    private readonly ServiceResult<object> serviceResult;

    public TestTypedUseCase(ServiceResult<object> serviceResult)
    {
        this.serviceResult = serviceResult;
    }
    protected override async Task<HandlerResult<Response>> HandleAsync([NotNull] Request request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return this.ExecutedResult(serviceResult);
    }
    public async Task<HandlerResult<Response>> Execute()
    {
        var request = new Request();
        return await HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
    }
}

internal class TestUntypedUseCase : UseCase<Request, EmptyResponse>
{
    private readonly ServiceResult serviceResult;

    public TestUntypedUseCase(ServiceResult serviceResult)
    {
        this.serviceResult = serviceResult;
    }
    protected override async Task<HandlerResult<EmptyResponse>> HandleAsync([NotNull] Request request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return this.ExecutedResult(serviceResult);
    }
    public async Task<HandlerResult<EmptyResponse>> Execute()
    {
        var request = new Request();
        return await HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
    }
}

internal class Request
{

}
internal class Response :
    MessageBusResponse<object>
{

}
internal class EmptyResponse :
    EmptyMessageBusResponse
{

}

internal class Outbox
{

}
