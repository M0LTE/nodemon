using nodemon.Configuration;
using nodemon.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("nodemon.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("/etc/nodemon.json", optional: true, reloadOnChange: true);
builder.Services.Configure<NodeMonConfig>(builder.Configuration.GetSection(nameof(NodeMonConfig)));

builder.Services.AddSingleton<Arduino>();
builder.Services.AddHostedService<ArduinoManager>();
builder.Services.AddHostedService<TaitManager>();

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
