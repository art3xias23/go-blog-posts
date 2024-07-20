using Microsoft.AspNetCore.Routing.Patterns;
using Scriban;
using Scriban.Runtime;

namespace blog_2_charp_urlfor
{
    public class TemplateParser : ITemplateParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly EndpointDataSource _dataSource;

        public TemplateParser(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, EndpointDataSource dataSource)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _dataSource = dataSource;
        }
        public string Render(object model, string filePath)
        {
            var template = Template.Parse(filePath);

            var context = new TemplateContext
            {
                MemberRenamer = member => member.Name
            };
            var scriptObject = new ScriptObject();

            if (model != null)
            {
                scriptObject.Import(model);
            }

            scriptObject.Import("url_for", new Func<string, object?[], string>((name, parameters) =>
                UrlFor(_linkGenerator, _httpContextAccessor.HttpContext, name, parameters)));

            context.PushGlobal(scriptObject);
            return template.Render(context);
        }

        public string RenderFile(object model, string filePath)
        {
            var contents = File.ReadAllText(filePath);
            var template = Template.Parse(contents);

            var context = new TemplateContext
            {
                MemberRenamer = member => member.Name
            };
            var scriptObject = new ScriptObject();

            if (model != null)
            {
                scriptObject.Import(model);
            }

            scriptObject.Import("url_for", new Func<string, object?[], string>((name, parameters) =>
                UrlFor(_linkGenerator, _httpContextAccessor.HttpContext, name, parameters)));

            context.PushGlobal(scriptObject);
            return template.Render(context);
        }

        private string UrlFor(LinkGenerator linkGenerator, HttpContext httpContext, string endpointName, object?[] providedRouteValues)
        {
            var endPoint = _dataSource.Endpoints.First(x => x.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == endpointName);

            if (endPoint is RouteEndpoint routeEndpoint)
            {
                var routeValues = ExtractRouteValues(routeEndpoint.RoutePattern, providedRouteValues);

                if (routeValues.Count() != providedRouteValues.Length)
                {
                    throw new ArgumentException(
                        $"Endpoint ({endpointName}'s expected route values({routeValues.Count} don't match provided route values({providedRouteValues.Length})");
                }


                var newPath = linkGenerator.GetPathByName(httpContext, endpointName, routeValues);
                return newPath;
            }

            return "/error";
        }

        private IDictionary<string, string> ExtractRouteValues(RoutePattern pattern, object[]? providedRouteValues)
        {
            int count = 0;
            var routeValues = new Dictionary<string, string>();
            foreach (var part in pattern.PathSegments.SelectMany(segment => segment.Parts))
            {
                if (part is RoutePatternParameterPart parameterPart)
                {
                    routeValues[parameterPart.Name] = providedRouteValues[count].ToString();
                    count++;
                }
            }

            return routeValues;
        }
    }
}
