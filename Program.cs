using TuesberryAPIServer;
using TuesberryAPIServer.Services;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddLogging();
builder.Services.AddControllers();

// dbconfig option binding
builder.Services.Configure<DbConfig>(builder.Configuration.GetSection(nameof(DbConfig)));

// add db services
builder.Services.AddTransient<IGameDb, GameDb>();
builder.Services.AddTransient<IAccountDb, AccountDb>();
builder.Services.AddSingleton<IMemoryDb, RedisDb>();

// log ¼³Á¤
builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole();

// build app
var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    
}

app.UseRouting();

app.MapControllers();

app.Run();
