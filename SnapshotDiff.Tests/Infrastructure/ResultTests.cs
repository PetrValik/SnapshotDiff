using FluentAssertions;
using SnapshotDiff.Infrastructure.Common;

namespace SnapshotDiff.Tests.Infrastructure;

public class ResultTests
{
    // ── Result<T> success ────────────────────────────────────────────────────

    [Fact]
    public void Success_IsSuccess_IsTrue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_Value_ReturnsCorrectValue()
    {
        var result = Result<string>.Success("hello");

        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Success_AccessingError_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Success(1);

        var act = () => result.Error;

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Result<T> failure ────────────────────────────────────────────────────

    [Fact]
    public void Failure_IsSuccess_IsFalse()
    {
        var result = Result<int>.Failure("oops");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Failure_Error_ReturnsMessage()
    {
        var result = Result<int>.Failure("something went wrong");

        result.Error.Should().Be("something went wrong");
    }

    [Fact]
    public void Failure_AccessingValue_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Failure("err");

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    // ── implicit conversion ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<string> result = "converted";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("converted");
    }

    // ── Match ────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_OnSuccess_CallsOnSuccess()
    {
        var result = Result<int>.Success(10);

        var output = result.Match(
            onSuccess: v => $"value={v}",
            onFailure: _ => "failed");

        output.Should().Be("value=10");
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailure()
    {
        var result = Result<int>.Failure("broken");

        var output = result.Match(
            onSuccess: _ => "ok",
            onFailure: e => $"err:{e}");

        output.Should().Be("err:broken");
    }

    [Fact]
    public void Match_NullOnSuccess_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(1);

        var act = () => result.Match(null!, _ => "");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Match_NullOnFailure_ThrowsArgumentNullException()
    {
        var result = Result<int>.Failure("e");

        var act = () => result.Match(_ => "", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── MatchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task MatchAsync_OnSuccess_CallsOnSuccess()
    {
        var result = Result<int>.Success(5);

        var output = await result.MatchAsync(
            onSuccess: v => Task.FromResult(v * 2),
            onFailure: _ => Task.FromResult(-1));

        output.Should().Be(10);
    }

    [Fact]
    public async Task MatchAsync_OnFailure_CallsOnFailure()
    {
        var result = Result<int>.Failure("err");

        var output = await result.MatchAsync(
            onSuccess: _ => Task.FromResult(0),
            onFailure: e => Task.FromResult(e.Length));

        output.Should().Be(3);
    }

    // ── Map ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(5);

        var mapped = result.Map(v => v.ToString());

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("5");
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var result = Result<int>.Failure("original error");

        var mapped = result.Map(v => v.ToString());

        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be("original error");
    }

    [Fact]
    public void Map_NullMapper_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(1);

        var act = () => result.Map<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Bind ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSuccess_AppliesBinder()
    {
        var result = Result<int>.Success(3);

        var bound = result.Bind(v => v > 0
            ? Result<string>.Success("positive")
            : Result<string>.Failure("non-positive"));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("positive");
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        var result = Result<int>.Failure("base error");

        var bound = result.Bind(v => Result<string>.Success(v.ToString()));

        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().Be("base error");
    }

    [Fact]
    public void Bind_NullBinder_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(1);

        var act = () => result.Bind<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Result (non-generic) ─────────────────────────────────────────────────

    [Fact]
    public void ResultSuccess_IsSuccess_IsTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ResultSuccess_AccessingError_Throws()
    {
        var result = Result.Success();

        var act = () => result.Error;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultFailure_IsSuccess_IsFalse()
    {
        var result = Result.Failure("error msg");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error msg");
    }

    [Fact]
    public void ResultMatch_OnSuccess_CallsOnSuccess()
    {
        var result = Result.Success();

        var output = result.Match(
            onSuccess: () => "ok",
            onFailure: _ => "fail");

        output.Should().Be("ok");
    }

    [Fact]
    public void ResultMatch_OnFailure_CallsOnFailure()
    {
        var result = Result.Failure("bad");

        var output = result.Match(
            onSuccess: () => "ok",
            onFailure: e => e);

        output.Should().Be("bad");
    }

    [Fact]
    public void ResultMatch_NullOnSuccess_ThrowsArgumentNullException()
    {
        var result = Result.Success();

        var act = () => result.Match<string>(null!, _ => "");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ResultMatch_NullOnFailure_ThrowsArgumentNullException()
    {
        var result = Result.Failure("e");

        var act = () => result.Match<string>(() => "", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Theory ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Success_Theory_ValueRoundtrips(int value)
    {
        var result = Result<int>.Success(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("error one")]
    [InlineData("another error")]
    [InlineData("")]
    public void Failure_Theory_ErrorRoundtrips(string error)
    {
        var result = Result<string>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }
}
