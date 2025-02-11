import OpenAI from "openai";
import { RunnerResponse, defaultModel, Tool } from "../functions.js";
import { Task } from "../todo-api.js";
import { randomUUID } from "crypto";

export class addTask extends Tool {
  static definition = {
    name: "add_task",
    description: "This function creates a new task for the current user.",
    parameters: {
      type: "object",
      properties: {
        username: { type: "string", description: "The username of the user." },
        title: { type: "string", description: "The title of the task." },
        description: { type: "string", description: "The description of the task." },
        dueDate: { type: "string", format: "date-time", description: "The due date of the task." },
        completed: { type: "boolean", description: "The completion status of the task." },
      },
      required: ["title"],
    },
  };

  async execute(
    messages: OpenAI.ChatCompletionMessageParam[],
    args: { username: string, title: string; description?: string; dueDate?: string, completed?: boolean }
  ): Promise<RunnerResponse> {

    const newTask = await this.todoAPI.createTask({
            id: randomUUID(),
            title: args.title,
            userId: args.username,
            description: args.description || "",
            dueDate: args.dueDate,
            completed: false,
        });

    const systemMessage = [
      "A new task has been created successfully.",
      `Id: ${newTask.id}`,
      `Title: ${newTask.title}`,
      `Description: ${newTask.description}`,
      `Due Date: ${newTask.dueDate}`,
      `Completed: ${newTask.completed}`,
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
