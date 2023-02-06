#region Imports
using CLIMATE_REST_API.Models;
using CLIMATE_REST_API.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
#endregion

#region Instanciate {builder} Variable From WebApplication Class
var builder = WebApplication.CreateBuilder(args);
#endregion

#region Add Services To The Container 
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBServices>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Register Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins("https://localhost:44351", "https://localhost:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});

#endregion

#region Adding Authentication


#endregion

#region Instanciate {app} Variable With Build() From {builder} variable
var app = builder.Build();
#endregion

#region Configure The HTTP Request Pipeline & Control When Swagger Is In Use
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Enable Cors
app.UseCors(builder =>
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