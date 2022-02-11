using System.Text;
using FluentAssertions;
using Xunit;

namespace Aoraki.Web.Tests;

public class StringWriterWithEncodingTests
{
    [Fact]
    public void Encoding_ShouldMatchConstructedValue()
        => new StringWriterWithEncoding(Encoding.Latin1).Encoding.Should().Be(Encoding.Latin1);
}