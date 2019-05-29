using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace citr.Infrastructure
{
    public static class UrlExtensions
    {
        public static string AbsoluteContent(this IUrlHelper url, string contentPath)
        {
            HttpRequest request = url.ActionContext.HttpContext.Request;
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
        }
    }
}
