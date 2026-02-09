var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/api/health", () => new
{
    Status = "OK",
    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    Environment = app.Environment.EnvironmentName,
    DotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
});

app.Run();
