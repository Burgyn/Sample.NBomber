using AutoBogus;
using AutoBogus.Conventions;

var builder = WebApplication.CreateBuilder(args);

AutoFaker.Configure(builder => { builder.WithConventions(); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapPost("/api/login", async (User User) =>
{
    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(150)));
    return Guid.NewGuid().ToString();
});

app.MapGet("/api/projects", async () =>
{
    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(150)));

    return AutoFaker.Generate<Project>(new Random().Next(1, 20));
});

app.MapGet("/api/projects/{id}", async (int id) =>
{
    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(150)));

    var p = AutoFaker.Generate<Project>();
    p.Id = id;

    return p;
});

app.Run();