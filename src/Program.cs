using System.Net.Http.Headers;
using System.Net.Http.Json;
using NBomber.Contracts;
using NBomber.CSharp;


// ------------------- initialization ----------------------

// Fake load. (in real world read from config)
List<User> users = new()
{
    new("user1", "pswd1"), new("user2", "pswd2"), new("user3", "pswd3"), 
    new("user4", "pswd4"), new("user5", "pswd5")
};

// Create 5 http clients for 5 real users with token
var httpFactory = ClientFactory.Create(
    "http_factory",
    clientCount: 5,
    initClient: async (number, _) =>
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5128");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await GetUserToken(number));
        return client;
    });

async Task<string> GetUserToken(int userIndex)
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5128");

    var response = await client.PostAsJsonAsync("/api/login", users[userIndex]);

    return await response.Content.ReadAsStringAsync();
}

// ----------------- load tests definition --------------------------

var getListStep = Step.Create("Get projects list", httpFactory, async context =>
{
    var response = await context.Client.GetAsync("/api/projects", context.CancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        return Response.Fail(statusCode: (int)response.StatusCode);
    }

    var projects = await response.Content.ReadFromJsonAsync<IEnumerable<Project>>();

    return Response.Ok(statusCode: (int)response.StatusCode, payload: projects!.First().Id);
});

var getProjectStep = Step.Create("Get project", httpFactory, async context =>
{
    var id = context.GetPreviousStepResponse<int>();
    var response = await context.Client.GetAsync($"/api/projects/{id}", context.CancellationToken);

    return response.IsSuccessStatusCode
        ? Response.Ok(statusCode: (int)response.StatusCode)
        : Response.Fail(statusCode: (int)response.StatusCode);
});

var scenario = ScenarioBuilder.CreateScenario("Get projects", getListStep, getProjectStep)
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.InjectPerSec(20, TimeSpan.FromSeconds(10)));

// -------------------- start load tests ---------------------------

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();