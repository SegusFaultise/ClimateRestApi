#region Imports
using CLIMATE_REST_API.Models;
using CLIMATE_REST_API.Services;
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

#region Instanciate {app} Variable With Build() From {builder} variable
var app = builder.Build();
#endregion

#region Configure The HTTP Request Pipeline & Control When Swagger Is In Use
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
#endregion