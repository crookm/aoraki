using System.IO;
using System.Text;

namespace Aoraki.Web;

/// <summary>
/// Used in the XML feed endpoints to configure a <see cref="StringWriter"/> with an encoding, as the XML writer does
/// not allow this.
/// </summary>
public sealed class StringWriterWithEncoding : StringWriter
{
    public StringWriterWithEncoding(Encoding encoding)
    {
        Encoding = encoding;
    }

    public override Encoding Encoding { get; }
}