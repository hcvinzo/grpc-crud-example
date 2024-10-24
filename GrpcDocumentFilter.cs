using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GrpcCrudExample;

public class GrpcDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                if (path.Key.StartsWith("/v1/greeter"))
                {
                    operation.Value.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Greeter" } };
                }
                else if (path.Key.StartsWith("/v1/auth"))
                {
                    operation.Value.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Auth" } };
                }
                else if (path.Key.StartsWith("/v1/orders"))
                {
                    operation.Value.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "OrderImporter" } };
                }
            }
        }
    }
}
