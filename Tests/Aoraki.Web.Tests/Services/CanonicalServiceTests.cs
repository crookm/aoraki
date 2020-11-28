using System;
using Aoraki.Web.Services;
using FluentAssertions;
using Xunit;

namespace Aoraki.Web.Tests.Services
{
    public class CanonicalServiceTests
    {
        [Theory]
        [InlineData("example.com", true, true, "http://example.com/")]
        [InlineData("example.com", true, true, "https://example.com/")]
        [InlineData("example.com", true, true, "https://example.com/ok/hi/sup/")]
        [InlineData("example.com", false, true, "https://example.com")]
        [InlineData("example.com", false, true, "https://example.com/ok/hi/sup")]
        // Uppercase
        [InlineData("example.com", true, false, "https://example.com/OK/AllowMeUpcase/")]
        [InlineData("example.com", true, false, "https://example.com/upcaseallowed/butidonthaveany/")]
        [InlineData("example.com", false, false, "https://example.com/OK/AllowMeUpcase/butnotrailingslash")]
        [InlineData("example.com", false, false, "https://example.com/upcaseallowed/butidonthaveany/alsonotrailingslash")]
        // Params
        [InlineData("example.com", true, true, "https://example.com/?interesting=withparams")]
        [InlineData("example.com", true, true, "https://example.com/and/some/path/?more=paramsplz")]
        [InlineData("example.com", false, true, "https://example.com?interesting=withparams")]
        [InlineData("example.com", false, true, "https://example.com/and/some/path?more=paramsplz")]
        [InlineData("example.com", false, false, "https://example.com/OK/AllowMeUpcase/butnotrailingslash?however=params")]
        // Fragments
        [InlineData("example.com", false, true, "https://example.com#unga")]
        [InlineData("example.com", true, true, "https://example.com/#headone")]
        [InlineData("example.com", true, true, "https://example.com/ok/hi/sup/#headtwo")]
        [InlineData("example.com", true, false, "https://example.com/OK/AllowMeUpcase/#ohwow")]
        [InlineData("example.com", true, true, "https://example.com/?interesting=withparams#hahahahaha")]
        // Subdomains
        [InlineData("www.example.com", true, true, "https://www.example.com/")]
        [InlineData("asda.ok.wow.example.com", true, true, "https://asda.ok.wow.example.com/")]
        [InlineData("www.example.com", false, true, "https://www.example.com")]
        [InlineData("asda.ok.wow.example.com", false, true, "https://asda.ok.wow.example.com")]
        // Ports
        [InlineData("example.com", true, true, "https://example.com:9090/")]
        [InlineData("example.com", false, true, "https://example.com:9090")]
        [InlineData("www.example.com", true, true, "https://www.example.com:10222/")]
        [InlineData("asda.ok.wow.example.com", false, true, "https://asda.ok.wow.example.com:9999")]
        [InlineData("example.com", true, true, "https://example.com:1234/ok/hi/sup/")]
        public void CanonicaliseUrl_ShouldReturnSameResult_WhenCanonical(
            string hostName, bool enableTrailingSlash, bool enableLowerCase, string inputUrl)
        {
            var service = new CanonicalService
            {
                HostName = hostName,
                EnableTrailingSlash = enableTrailingSlash,
                EnableLowerCase = enableLowerCase,
            };

            var result = service.CanonicaliseUrl(inputUrl);

            result.Should().Be(expected: inputUrl);
        }

        [Theory]
        // Default ports
        [InlineData("http://example.com:80", true)]
        [InlineData("https://example.com:443", true)]
        [InlineData("https://example.com:443/okok", true)]
        [InlineData("https://example.com:443/okok?sadkh=sdf", true)]
        [InlineData("https://sdfsdf.ads.example.com:443/okok?sadkh=sdf", true)]
        // Non-default Ports
        [InlineData("http://example.com:123", false)]
        [InlineData("https://example.com:456", false)]
        [InlineData("https://example.com:789/okok", false)]
        [InlineData("https://example.com:4444/okok?sadkh=sdf", false)]
        [InlineData("https://sdfsdf.ads.example.com:45676/okok?sadkh=sdf", false)]
        public void CanonicaliseUrl_ShouldRemovePort_WhenDefault(string inputUrl, bool expectedPortRemoved)
        {
            var service = new CanonicalService
            {
                HostName = "example.com",
                EnableTrailingSlash = true,
                EnableLowerCase = true,
            };

            var result = service.CanonicaliseUrl(inputUrl);

            if (expectedPortRemoved)
                result.Should().NotMatchRegex(@":\d+");
            else
                result.Should().MatchRegex(@":\d+");
        }

        [Theory]
        [InlineData("https://example.com?Testing=TruE", "https://example.com?testing=TruE")]
        [InlineData("https://example.com?Testing=TruE,oK", "https://example.com?testing=TruE,oK")]
        [InlineData("https://example.com?One=ONE&TWO=two&thREE=thRee", "https://example.com?one=ONE&two=two&three=thRee")]
        [InlineData("https://example.com?TUSHGFJKH=928364JHSGE", "https://example.com?tushgfjkh=928364JHSGE")]
        public void CanonicaliseUrl_ShouldLowercaseQueryKey_ButNotQueryValue(string inputUrl, string expectedResult)
        {
            var parsedHost = new Uri(inputUrl).Host;
            var service = new CanonicalService
            {
                HostName = parsedHost,
                EnableTrailingSlash = false,
                EnableLowerCase = true,
            };

            var result = service.CanonicaliseUrl(inputUrl);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("https://example.com/", "https://example.com")]
        [InlineData("https://example.com///", "https://example.com")]
        [InlineData("https://example.com/wesa/sdf/", "https://example.com/wesa/sdf")]
        [InlineData("https://example.com/wesa/sdf//////", "https://example.com/wesa/sdf")]
        [InlineData("https://example.com/wesa//sdf//////", "https://example.com/wesa//sdf")]
        [InlineData("https://example.com/wesa/sdf//////?okokok=yes", "https://example.com/wesa/sdf?okokok=yes")]
        [InlineData("https://example.com/wesa/sdf//////?okokok=yes#yoyo", "https://example.com/wesa/sdf?okokok=yes#yoyo")]
        [InlineData("https://example.com:1234/wesa/sdf//////?okokok=yes", "https://example.com:1234/wesa/sdf?okokok=yes")]
        public void CanonicaliseUrl_ShouldTrimTrailingSlashes_WhenEnabled(string inputUrl, string expectedResult)
        {
            var parsedHost = new Uri(inputUrl).Host;
            var service = new CanonicalService
            {
                HostName = parsedHost,
                EnableTrailingSlash = false,
                EnableLowerCase = true,
            };

            var result = service.CanonicaliseUrl(inputUrl);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void CanonicaliseUrl_ShouldProduceResult_WhenCallingOverrides()
        {
            var inputUrl = new Uri("https://example.com/Ok/Hi/");
            var expectedResult = "https://example.com/ok/hi";

            var service = new CanonicalService
            {
                HostName = "example.com",
                EnableTrailingSlash = false,
                EnableLowerCase = true,
            };

            var result = service.CanonicaliseUrl(inputUrl);

            result.Should().Be(expectedResult);
        }
    }
}