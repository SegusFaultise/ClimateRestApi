#region Imports
using CLIMATE_REST_API.Models;
using CLIMATE_REST_API.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
#endregion

#region Instanciate {builder} Variable From WebApplication Class
var builder =
WebApplication.CreateBuilder(args);
#endregion

#region Add Services To The Container 
builder.Services.Configure<MongoDBSettings>(
builder.Configuration.GetSection(
"MongoDB"));

builder.Services.AddSingleton<MongoDBServices>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
    "v1",
    new OpenApiInfo
    {
        Version = 
        "v1.0",
        Title =
        "Sensor Data API",
        Description =
        @"An edcucational api for interfacing with sensor data
        from IOT devices using mongoDB and C#.."
    });

    var xmlFile =
    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
    Path.Combine(
    AppContext.BaseDirectory,
    xmlFile);

    c.IncludeXmlComments(xmlPath);
});
#endregion

#region Register Cors
builder.Services.AddCors(
options =>
{
    options.AddDefaultPolicy(
    builder =>
        {
            builder.WithOrigins(
            "https://localhost:44351",
            "https://localhost:4200",
            "https://www.google.com/")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

#endregion

#region Instanciate {app} Variable With Build() From {builder} variable
var app =
builder.Build();
#endregion

#region Configure The HTTP Request Pipeline & Control When Swagger Is In Use
app.UseSwagger();
app.UseSwaggerUI();

#region Enable Cors
app.UseCors(
builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});
#endregion

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
#endregion