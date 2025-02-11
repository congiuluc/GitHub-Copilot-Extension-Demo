import OpenAI from "openai";
import { RunnerResponse, defaultModel, Tool } from "../functions.js";
import { Task } from "../todo-api.js";
import { randomUUID } from "crypto";

export class updateTask extends Tool {
  static definition = {
    name: "update_task",
    description: "This function updates a task for the current user.",
    parameters: {
      type: "object",
      properties: {
        id: { type: "string", description: "The id of the task." },
        username: { type: "string", description: "The username of the user." },
        title: { type: "string", description: "The title of the task." },
        description: { type: "string", description: "The description of the task." },
        dueDate: { type: "string", format: "date-time", description: "The due date of the task." },
        completed: { type: "boolean", description: "The completion status of the task." },
      },
      required: ["id","title"],
    },
  };

  async execute(
    messages: OpenAI.ChatCompletionMessageParam[],
    args: { id: string, username: string, title: string; description?: string; dueDate?: string, completed?: boolean }
  ): Promise<RunnerResponse> {

    const task = await this.todoAPI.updateTask({
            id: args.id,
            title: args.title,
            userId: args.username,
            description: args.description || "",
            dueDate: args.dueDate,
            completed: args.completed,
        });

    const systemMessage = [
      "Task updated successfully.",
      `Id: ${task.id}`,
      `Title: ${task.title}`,
      `Description: ${task.description}`,
      `Due Date: ${task.dueDate}`,
      `Completed: ${task.completed}`,
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
