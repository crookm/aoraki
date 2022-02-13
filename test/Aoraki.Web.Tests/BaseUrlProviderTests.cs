using FluentAssertions;
using Xunit;

namespace Aoraki.Web.Tests;

public class BaseUrlProviderTests
{
    [Fact]
    public void BaseUrl_ShouldBeAccurate()
        => new BaseUrlProvider().BaseUrl.Host.Should().Be(Constants.SiteHostName);
}