import { createServer, IncomingMessage } from "http";

import { verifyAndParseRequest, transformPayloadForOpenAICompatibility, createReferencesEvent } 
from "@copilot-extensions/preview-sdk";
import OpenAI from "openai";

import { RunnerResponse } from "./functions.js";
import { addTask } from "./functions/add-task.js";
import { listTasks } from "./functions/list-tasks.js";
import { updateTask } from "./functions/update-task.js";
import { deleteTask } from "./functions/delete-task.js";
import { getWeather } from "./functions/get-weather.js";
import { getTask } from "./functions/get-task.js";
import { TodoAPI } from "./todo-api.js";
import { env } from "process";

const server = createServer(async (request, response) => {
  // Handle GET requests
  if (request.method === "GET") {
    response.statusCode = 200;
    response.end(`OK`);
    return;
  }

  // Get the request body
  const body = await getBody(request);

  // Verify and parse the request
  let verifyAndParseRequestResult: Awaited<ReturnType<typeof verifyAndParseRequest>>;
  const apiKey = request.headers["x-github-token"] as string;
  try {
    const signature = request.headers["x-github-public-key-signature"] as string;
    const keyID = request.headers["x-github-public-key-identifier"] as string;
    verifyAndParseRequestResult = await verifyAndParseRequest(body, signature, keyID, {
      token: apiKey,
    });
  } catch (err) {
    console.error(err);
    response.statusCode = 401
    response.end("Unauthorized");
    return
  }

  const { isValidRequest, payload } = verifyAndParseRequestResult

  // Check if the request is valid
  if (!isValidRequest) {
    console.log("Signature verification failed");
    response.statusCode = 401
    response.end("Unauthorized");
  }

  console.log("Signature verified");

  // Transform the payload for OpenAI compatibility
  const compatibilityPayload = transformPayloadForOpenAICompatibility(payload);

  // Check if the GitHub API token is provided
  if (!apiKey) {
    response.statusCode = 400
    response.end()
    return;
  }
  const apiBaseURL = env.API_BASE_URL || "https://api.github.com";
  
  // List of functions that are available to be called
  const todoAPI = new TodoAPI(apiBaseURL, apiKey);
  const functions = [listTasks, addTask, updateTask, deleteTask, getWeather, getTask];

  // Use the Copilot API to determine which function to execute
  const capiClient = new OpenAI({
    baseURL: "https://api.githubcopilot.com",
    apiKey,
  });

  const toolCallMessages = [
    {
      role: "system" as const,
      content: [
        "You are an extension of GitHub Copilot, built to interact with todo tasks",
      ].join("\n"),
    },
    ...compatibilityPayload.messages,
  ];

  console.time("tool-call");
  const toolCaller = await capiClient.chat.completions.create({
    stream: false,
    model: "gpt-4o",
    messages: toolCallMessages,
    tool_choice: "auto",
    tools: functions.map((f) => f.tool),
  });
  console.timeEnd("tool-call");

  // Check if a tool call was found
  if (
    !toolCaller.choices[0] ||
    !toolCaller.choices[0].message ||
    !toolCaller.choices[0].message.tool_calls ||
    !toolCaller.choices[0].message.tool_calls[0].function
  ) {
    console.log("No tool call found");
    // No tool to call, so just call the model with the original messages
    const stream = await capiClient.chat.completions.create({
      stream: true,
      model: "gpt-4o",
      messages: payload.messages,
    });

    for await (const chunk of stream) {
      const chunkStr = "data: " + JSON.stringify(chunk) + "\n\n";
      response.write(chunkStr);
    }
    response.write("data: [DONE]\n\n");
    response.end();
    return;
  }

  // A tool has been called, so we need to execute the tool's function
  const functionToCall = toolCaller.choices[0].message.tool_calls[0].function;
  const args = JSON.parse(functionToCall.arguments);

  console.time("function-exec");
  let functionCallRes: RunnerResponse;
  try {
    console.log("Executing function", functionToCall.name);
    const funcClass = functions.find(
      (f) => f.definition.name === functionToCall.name
    );
    if (!funcClass) {
      throw new Error("Unknown function");
    }

    console.log("\t with args", args);
    const func = new funcClass(todoAPI);
    // Execute the function with the provided arguments
    functionCallRes = await func.execute(payload.messages, args);
  } catch (err) {
    console.error(err);
    response.statusCode = 500
    response.end();
    return;
  }
  console.timeEnd("function-exec");

  // Stream the response from the Copilot API
  const stream = await capiClient.chat.completions.create({
    stream: true,
    model: "gpt-4o",
    messages: functionCallRes.messages,
  });

  console.time("streaming");
  for await (const chunk of stream) {
    const chunkStr = "data: " + JSON.stringify(chunk) + "\n\n";
    response.write(chunkStr);
  }
  response.write("data: [DONE]\n\n");
  console.timeEnd("streaming");
  response.end();
});

// Start the server
const port = process.env.PORT || "3000"
server.listen(port);
console.log(`Server running at http://localhost:${port}`);

// Function to get the body of the request
function getBody(request: IncomingMessage): Promise<string> {
  return new Promise((resolve) => {
    const bodyParts: any[] = [];
    let body;
    request
      .on("data", (chunk) => {
        bodyParts.push(chunk);
      })
      .on("end", () => {
        body = Buffer.concat(bodyParts).toString();
        resolve(body);
      });
  });
}
