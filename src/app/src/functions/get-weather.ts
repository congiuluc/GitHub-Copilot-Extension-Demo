import OpenAI from "openai";
import { RunnerResponse, defaultModel, Tool } from "../functions.js";
import fetch from "node-fetch";

export class getWeather extends Tool {
  static definition = {
    name: "get_weather",
    description: "This function fetches the weather forecast for a given location.",
    parameters: {
      type: "object",
      properties: {
        location: { type: "string", description: "The location to get the weather forecast for." },
        country: { type: "string", description: "The country code for the location." },
      },
      required: ["location"],
    },
  };

  async execute(
    messages: OpenAI.ChatCompletionMessageParam[],
    args: { location: string }
  ): Promise<RunnerResponse> {
    const apiKey = process.env.OPENWEATHERMAP_API_KEY;
    const response = await fetch(`http://api.openweathermap.org/data/2.5/weather?q=${args.location},${args.location}&appid=${apiKey}&units=metric`);
    const weatherData = await response.json();

    const systemMessage = [
      `Weather forecast for ${weatherData.name} (${weatherData.sys.country}):`,
      `Condition: ${weatherData.weather.description} (${weatherData.weather.main})`,
      `Temperature: ${weatherData.main.temp}°F`,
      `Humidity: ${weatherData.main.humidity}%`,
      `Wind: ${weatherData.wind.speed} kph`,
      'Sunrise: ' + new Date(weatherData.sys.sunrise * 1000).toLocaleTimeString(),
      'Sunset: ' + new Date(weatherData.sys.sunset * 1000).toLocaleTimeString(),
      `Icon: ![${weatherData.name} (${weatherData.sys.country}](https://openweathermap.org/img/wn/${weatherData.weather.icon}@2x.png)`,
      "Use markdown to show the current weather condition, temperature, humidity, wind speed, sunrise, and sunset time.",
      "Display the weather icon for the current condition.",

      
      ];

    return {
      model: defaultModel,
      messages: [
        { role: "system", content: systemMessage.join("\n") },
        ...messages,
      ],
    };
  }
}
