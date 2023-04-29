using TuesberryAPIServer;
using TuesberryAPIServer.Middleware;
using TuesberryAPIServer.Services;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddLogging();
builder.Services.AddControllers();

// http context accessor
builder.Services.AddHttpContextAccessor();

// dbconfig option binding
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));

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

// add check middleware
app.UseCheckUserVersion();
app.UseCheckUserAuth();

// use routing
app.UseRouting();

app.MapControllers();

//redis init
var redisDb = app.Services.GetRequiredService<IMemoryDb>();
redisDb.Init(configuration.GetSection("DbConfig")["Redis"]);

app.Run(configuration["ServerAddress"]);
