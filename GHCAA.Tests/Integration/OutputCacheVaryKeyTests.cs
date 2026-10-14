using FluentAssertions;
using GHCAA.API.Controllers;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OutputCaching;
using System.Reflection;

namespace GHCAA.Tests.Integration
{
    // PublicContent varies its cache key by a fixed list of query names. A cached action that reads
    // a name missing from the list would serve one cached answer for every value of it.
    [TestFixture]
    [Category("NFR-P1")]
    public class OutputCacheVaryKeyTests
    {
        private static IEnumerable<(string Action, ParameterInfo Parameter)> QueryParameters(string policy)
        {
            var controllers = typeof(EventsController).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var controller in controllers)
            {
                var prefixes = controller.GetCustomAttributes<RouteAttribute>().Select(r => r.Template).DefaultIfEmpty("").ToList();
                foreach (var method in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (method.GetCustomAttribute<OutputCacheAttribute>()?.PolicyName != policy) continue;
                    // An action can carry several routes. A parameter counts as a route value only
                    // when every one of them binds it; otherwise some URL sends it as a query.
                    var templates = method.GetCustomAttributes<HttpMethodAttribute>().Select(h => h.Template ?? "")
                        .SelectMany(t => prefixes.Select(p => p + "/" + t)).ToList();
                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameter.ParameterType == typeof(CancellationToken)) continue;
                        if (templates.All(t => t.Contains("{" + parameter.Name + "}") || t.Contains("{" + parameter.Name + ":"))) continue;
                        yield return ($"{controller.Name}.{method.Name}", parameter);
                    }
                }
            }
        }

        private static string QueryName(ParameterInfo p) =>
            p.GetCustomAttribute<FromQueryAttribute>()?.Name ?? p.Name!;

        [Test]
        public void PublicContentActions_VaryByEveryQueryNameTheyRead()
        {
            var found = QueryParameters(Constants.OutputCachePolicies.PublicContent).ToList();

            found.Should().NotBeEmpty("the check has to see the cached actions to mean anything");
            found.Where(f => !Constants.OutputCachePolicies.PublicContentQueryKeys.Contains(QueryName(f.Parameter), StringComparer.OrdinalIgnoreCase))
                .Select(f => $"{f.Action}({QueryName(f.Parameter)})")
                .Should().BeEmpty("each query name a PublicContent action reads must be in PublicContentQueryKeys");
        }

        // PublicReference does not vary by query at all, so its actions must not read one.
        [Test]
        public void PublicReferenceActions_ReadNoQueryString()
        {
            QueryParameters(Constants.OutputCachePolicies.PublicReference)
                .Select(f => $"{f.Action}({QueryName(f.Parameter)})")
                .Should().BeEmpty();
        }
    }
}
