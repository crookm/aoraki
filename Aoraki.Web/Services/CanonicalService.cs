using System;
using System.Collections.Generic;
using System.Text;
using Aoraki.Web.Contracts;
using Microsoft.AspNetCore.WebUtilities;

namespace Aoraki.Web.Services
{
    public class CanonicalService : ICanonicalService
    {
        public string HostName { get; set; }

        public bool EnableTrailingSlash { get; set; }
        public bool EnableLowerCase { get; set; }

        public string CanonicaliseUrl(string url)
        {
            return CanonicaliseUrl(new Uri(url));
        }

        public string CanonicaliseUrl(Uri uri)
        {
            var builder = new StringBuilder();
            builder.Append($"{uri.Scheme}://");
            builder.Append(HostName);

            if (!uri.IsDefaultPort)
                builder.Append($":{uri.Port}");

            // - Path
            var newPath = uri.AbsolutePath.TrimEnd('/');
            if (EnableLowerCase)
                newPath = newPath.ToLowerInvariant();

            if (EnableTrailingSlash)
                newPath += '/';

            builder.Append(newPath);

            // - Query parameters
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                var newQuery = new List<string>();
                foreach (var item in query)
                {
                    var key = item.Key;
                    if (EnableLowerCase) key = key.ToLowerInvariant();

                    newQuery.Add($"{key}={item.Value}");
                }

                builder.Append($"?{string.Join("&", newQuery)}");
            }

            builder.Append(uri.Fragment);
            return builder.ToString();
        }
    }
}