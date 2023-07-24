using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace Masters.Api
{
    public static class SwaggerConfiguration
    {      
        public  static  IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddVersionedApiExplorer(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
                
            });
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                
            });

            // string serviceDescription = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SeviceDescription.md"));
            services.AddSwaggerGen(c =>
           {
               c.EnableAnnotations();

               IApiVersionDescriptionProvider provider =
                 services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
               foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
               {
                   c.SwaggerDoc(description.GroupName, CrateInfoforApiVersion(description));
               }
               string xmlFile = $"{typeof(SwaggerConfiguration).Assembly.GetName().Name}.xml";

               c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
               c.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}_{e.ActionDescriptor.RouteValues["action"]}");
               c.ExampleFilters();
           });
            services.AddSwaggerExamplesFromAssemblyOf<Program>();

            return services;
        }
        //public static  WebApplication ConfigureSwagger(this WebApplication app,
        //                                                IApiVersionDescriptionProvider provider)
                                                         
        //{
        //    app.UseSwagger(
        //        options => options.RouteTemplate = $"swagger/{ApiConstants.ServiceName}/{{documentName}}/swagger.json");
        //    app.UseSwaggerUI(
        //        options =>
        //        {
        //            options.RoutePrefix = $"swagger/{ApiConstants.ServiceName}";
                    
        //            foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        //            {
        //                options.SwaggerEndpoint($"{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        //            }

        //        });
        //    return app ;
        //}
        private static OpenApiInfo CrateInfoforApiVersion(ApiVersionDescription description)
        {
            string serviceDescription = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ServiceDescription.md"));
            var info = new OpenApiInfo
            {
                Title = $"{ApiConstants.FriendlyServiceName}  API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = serviceDescription,
            };
            if(description.IsDeprecated)
            {
                info.Description = $"{Environment.NewLine} This Api Vesion has been Depriciated";
            }
            return info;
        }
    }   
}
