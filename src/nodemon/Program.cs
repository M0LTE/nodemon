using nodemon.Configuration;
using nodemon.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("nodemon.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("/etc/nodemon.json", optional: true, reloadOnChange: true);
builder.Services.Configure<NodeMonConfig>(builder.Configuration.GetSection(nameof(NodeMonConfig)));
builder.Services.AddLogging(options =>
{
    options.AddSimpleConsole(c =>
    {
        c.TimestampFormat = "[yyyy-MM-ddTHH:mm:ss] ";
        c.UseUtcTimestamp = true;
        c.SingleLine = true;
    });
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<ArduinoSingleton>();
builder.Services.AddHostedService<ArduinoManager>(); // must come before TaitManager
builder.Services.AddHostedService<TaitManager>();
builder.Services.AddRazorPages();

var app = builder.Build();
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapHub<NodeHub>("/nodeHub");
app.Run();
