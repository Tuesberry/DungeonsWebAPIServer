using TuesberryAPIServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    
}

app.UseRouting();

app.MapControllers();

IConfiguration configuration = app.Configuration;
DBManager.Init(configuration);  

app.Run();
