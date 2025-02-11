using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddMemoryCache();



builder.Services.AddHttpClient<GitHubService>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.github.com/");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/vnd.github.v3+json");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.UserAgent, "CopilotExtension");
});


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<TodoDbContext>(options =>
        options.UseInMemoryDatabase("TodoList"));
    builder.Services.AddScoped<ITodoRepository, InMemoryTodoRepository>();

}
else
{
    TokenCredential credential = new DefaultAzureCredential();
    var cosmosClient = new CosmosClient(
        accountEndpoint: builder.Configuration["COSMOS_ENDPOINT"],
        tokenCredential: credential
    );
    builder.Services.AddSingleton(cosmosClient);
    builder.Services.AddSingleton<ITodoRepository, CosmosTodoRepository>();
}

builder.Services.AddSingleton<RequestValidationService>();


var app = builder.Build();

// Register the middleware
//app.UseMiddleware<RequestValidationMiddleware>();

var logger = app.Logger;

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Map Todo API endpoints
app.MapGet("/", () => Results.Text("OK")).WithName("GetRoot");

app.MapTodoEndpoints();
app.MapTodoSkillsetEndpoints();
app.MapWeatherEndpoints();

app.Run();

