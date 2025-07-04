using Azure.Storage.Blobs;
using myapp_back.azure.interfaces;
using myapp_back.azure.services;
using myapp_back.config;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configure AzureOptions from appsettings.json
builder.Services.Configure<AzureOptions>(
    builder.Configuration.GetSection("AzureOptions")
);

// 🔹 Register BlobServiceClient BEFORE AzureService
builder.Services.AddSingleton(provider =>
{
    var azureOptions = provider
        .GetRequiredService<IConfiguration>()
        .GetSection("AzureOptions")
        .Get<AzureOptions>();

    return new BlobServiceClient(
        $"DefaultEndpointsProtocol=https;AccountName={azureOptions.AccountName};AccountKey={azureOptions.AccountKey};EndpointSuffix=core.windows.net"
    );
});

// 🔹 Register your Azure service AFTER BlobServiceClient
builder.Services.AddScoped<IAzureService, AzureService>();

// 🔹 CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
