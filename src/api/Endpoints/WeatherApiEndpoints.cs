using System.Drawing.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class WeatherEndpoints
{
    public static void MapWeatherEndpoints(this WebApplication app)
    {
        var logger = app.Logger;
        var httpClient = app.Services.GetRequiredService<HttpClient>();
        var Configuration = app.Services.GetRequiredService<IConfiguration>();
        app.MapPost("/api/weather", async (WeatherDataRequest weatherRequest, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            // Get weather data from the city using open weather api
            logger.LogInformation("Getting weather data for city: {city}", weatherRequest.City);
            var apiKey = Configuration["OPENWEATHERMAP_API_KEY"];
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={weatherRequest.City}&appid={apiKey}&units=metric";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonConvert.DeserializeObject<WeatherData>(content);
                logger.LogInformation("Weather data retrieved successfully.");
                return Results.Ok(weatherData);
            }
            else
            {
                logger.LogError("Failed to get weather data. Status code: {statusCode}", response.StatusCode);
                return Results.BadRequest("Failed to get weather data.");
            }

        })
        .WithName("GetSkillsetWeatherData");

        app.MapGet("/api/weather", async (string city, string? country, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            // Get weather data from the city using open weather api
            logger.LogInformation("Getting weather data for city: {city}", city);
            var apiKey = Configuration["OPENWEATHERMAP_API_KEY"];
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city},{country}&appid={apiKey}&units=metric";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonConvert.DeserializeObject<WeatherData>(content);
                logger.LogInformation("Weather data retrieved successfully.");
                return Results.Ok(weatherData);
            }
            else
            {
                logger.LogError("Failed to get weather data. Status code: {statusCode}", response.StatusCode);
                return Results.BadRequest("Failed to get weather data.");
            }

        })
        .WithName("GetWeatherData");
    }
}
