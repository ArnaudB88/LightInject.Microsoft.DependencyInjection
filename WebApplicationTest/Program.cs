using LightInject;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

#region custom code for lightinject test
builder.Host
    .UseLightInject()
    .ConfigureContainer<ServiceContainer>(container =>
    {
        ConfigureContainer(container);
        ConfigureServicesWithContainer(container, builder.Services);
    });

#endregion custom code for lightinject test

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
////builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen()
//    .AddSwaggerGenNewtonsoftSupport();

var app = builder.Build();

//Debug: in VS
//breakpoint in de ConsoleLoggerProvider constructor
//var test = new ConsoleLoggerProvider();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();


#region custom code for lightinject test
void ConfigureContainer(IServiceContainer container)
{
    container.RegisterSingleton<ApiInformation>(factory => new());
}

void ConfigureServicesWithContainer(IServiceContainer container, IServiceCollection services)
{
    var apiInformation = new ApiInformation();
    //How to test the bug?
    //Uncomment line below
    //apiInformation = container.GetInstance<ApiInformation>();


    var swaggerDocuments = new Dictionary<string, OpenApiInfo>();
    swaggerDocuments.Add("1.0", new OpenApiInfo
    {
        //Title = apiInformation.Title,
        Version = "1.0",
        Description = apiInformation.Description,
        //Contact = new OpenApiContact { Name = apiInformation.Contact?.Name, Email = apiInformation.Contact?.Email },
        //TermsOfService = !apiInformation.TermsOfServiceUri.IsNullOrWhitespace() ? new System.Uri(apiInformation.TermsOfServiceUri) : null,
        //License = !apiInformation.LicenseUri.IsNullOrWhitespace() ? new OpenApiLicense { Name = apiInformation.LicenseName, Url = new System.Uri(apiInformation.LicenseUri) } : null
    });

    builder.Services.AddSwaggerGen(c =>
    {
        foreach (var swaggerDoc in swaggerDocuments)
            c.SwaggerDoc(swaggerDoc.Key, swaggerDoc.Value);
    });
}

public class ApiInformation
{
    public string Description { get; set; }
}

#endregion custom code for lightinject test