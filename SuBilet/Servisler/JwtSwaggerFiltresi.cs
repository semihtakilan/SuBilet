using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SuBilet.Servisler;

// Swagger UI'da yalnızca [Authorize] olan uç noktalara kilit simgesi koyar.
// Sağ üstteki "Authorize" düğmesine /api/hesap/giris'ten alınan token yapıştırılır.
public class JwtSwaggerFiltresi : IOperationFilter
{
    public const string SemaAdi = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        bool girisGerekli = metadata.OfType<IAuthorizeData>().Any()
            && !metadata.OfType<IAllowAnonymous>().Any();
        if (!girisGerekli)
            return;

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SemaAdi }
            }] = Array.Empty<string>()
        });
    }
}
