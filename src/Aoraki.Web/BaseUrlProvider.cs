using System;
using SimpleMvcSitemap.Routing;

namespace Aoraki.Web;

public class BaseUrlProvider : IBaseUrlProvider
{
    public Uri BaseUrl => new(Constants.SiteBaseUrl);
}