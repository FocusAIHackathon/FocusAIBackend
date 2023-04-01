using Newtonsoft.Json.Serialization;
using Cloops.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddControllers().AddNewtonsoftJson(option => {
    option.SerializerSettings.DateFormatString = "dd-MM-yyyy HH:mm:ss";
    option.SerializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new DefaultNamingStrategy() };
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCLExceptionHandling();

app.MapControllers();

app.Run();
