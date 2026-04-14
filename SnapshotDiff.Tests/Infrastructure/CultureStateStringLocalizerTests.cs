using System.Globalization;
using Microsoft.Extensions.Localization;
using NSubstitute;
using SnapshotDiff.Infrastructure.Localization;

namespace SnapshotDiff.Tests.Infrastructure;

public class CultureStateStringLocalizerTests
{
    private readonly CultureState _state = new();

    private CultureStateStringLocalizer<FakeResource> CreateLocalizer()
    {
        var factory = Substitute.For<IStringLocalizerFactory>();
        var inner = Substitute.For<IStringLocalizer>();

        // Return the thread's current UI culture name so we can assert what the localizer saw
        inner[Arg.Any<string>()]
            .Returns(ci => new LocalizedString((string)ci[0], CultureInfo.CurrentUICulture.Name));

        inner[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(ci => new LocalizedString((string)ci[0], CultureInfo.CurrentUICulture.Name));

        factory.Create(typeof(FakeResource)).Returns(inner);

        return new CultureStateStringLocalizer<FakeResource>(factory, _state);
    }

    [Fact]
    public void SyncsCulture_WhenThreadHasStaleCulture()
    {
        _state.NotifyChanged("cs");

        // Simulate stale thread culture
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

        var localizer = CreateLocalizer();
        var result = localizer["Test"];

        Assert.Equal("cs", result.Value);
        Assert.Equal("cs", Thread.CurrentThread.CurrentUICulture.Name);
    }

    [Fact]
    public void SkipsSync_WhenCultureAlreadyCorrect()
    {
        _state.NotifyChanged("en");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

        var localizer = CreateLocalizer();
        var result = localizer["Test"];

        Assert.Equal("en", result.Value);
    }

    [Fact]
    public void SyncsForIndexerWithArguments()
    {
        _state.NotifyChanged("cs");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

        var localizer = CreateLocalizer();
        var result = localizer["Test", "arg1"];

        Assert.Equal("cs", result.Value);
    }

    [Fact]
    public void SyncsForGetAllStrings()
    {
        _state.NotifyChanged("cs");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

        var factory = Substitute.For<IStringLocalizerFactory>();
        var inner = Substitute.For<IStringLocalizer>();

        inner.GetAllStrings(Arg.Any<bool>())
            .Returns(_ => new[]
            {
                new LocalizedString("key", CultureInfo.CurrentUICulture.Name)
            });

        factory.Create(typeof(FakeResource)).Returns(inner);

        var localizer = new CultureStateStringLocalizer<FakeResource>(factory, _state);
        var result = localizer.GetAllStrings(false).First();

        Assert.Equal("cs", result.Value);
    }

    private sealed class FakeResource { }
}
