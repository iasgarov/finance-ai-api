using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Controllers
builder.Services.AddControllers();

builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services.AddHttpClient("ollama", client =>
{
    var ep = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")
             ?? builder.Configuration["Ollama:Endpoint"]
             ?? "http://localhost:11434/";
    client.BaseAddress = new Uri(ep);
    client.Timeout = TimeSpan.FromMinutes(2);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();