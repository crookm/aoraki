using System;

namespace Aoraki.Web.Contracts;

public interface ICanonicalService
{
    string HostName { get; set; }
    bool EnableTrailingSlash { get; set; }
    bool EnableLowerCase { get; set; }

    string CanonicaliseUrl(string url);
    string CanonicaliseUrl(Uri uri);
}