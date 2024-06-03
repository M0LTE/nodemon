using nodemon.Configuration;
using nodemon.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("nodemon.json", optional: true, reloadOnChange: true);
builder.Services.Configure<NodeMonConfig>(builder.Configuration.GetSection(nameof(NodeMonConfig)));
builder.Services.AddHostedService<TaitManager>();
builder.Services.AddHostedService<ArduinoManager>();
builder.Services.AddSingleton<Arduino>();

var app = builder.Build();
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
