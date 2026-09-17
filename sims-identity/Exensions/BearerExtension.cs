using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;

//inspiration of: https://stackoverflow.com/questions/79265776/how-to-add-jwt-token-support-globally-in-scalar-for-a-net-9-application

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptions;

    public BearerSecuritySchemeTransformer(IApiDescriptionGroupCollectionProvider apiDescriptions)
    {
        _apiDescriptions = apiDescriptions;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.AddComponent("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        });

        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var apiDescription = _apiDescriptions.ApiDescriptionGroups.Items
                    .SelectMany(group => group.Items)
                    .FirstOrDefault(description =>
                        string.Equals(description.RelativePath, path.Key.TrimStart('/'), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(description.HttpMethod, operation.Key.ToString(), StringComparison.OrdinalIgnoreCase));

                var metadata = apiDescription?.ActionDescriptor.EndpointMetadata;
                var requiresAuthorization = metadata?.OfType<IAuthorizeData>().Any() == true;
                var allowsAnonymous = metadata?.OfType<IAllowAnonymous>().Any() == true;

                if (requiresAuthorization && !allowsAnonymous)
                {
                    operation.Value.Security ??= [];
                    operation.Value.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                }
            }
        }

        return Task.CompletedTask;
    }
}
