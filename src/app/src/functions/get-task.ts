import OpenAI from "openai";
import { RunnerResponse, defaultModel, Tool } from "../functions.js";
import { Task } from "../todo-api.js";

export class getTask extends Tool {
  static definition = {
    name: "get_task",
    description: "This function fetches a single task for the current user.",
    parameters: {
      type: "object",
      properties: {
        id: { type: "string", description: "The id of the task." }
      },
      required: ["id"],
    },
  };

  async execute(
    messages: OpenAI.ChatCompletionMessageParam[],
    args: { id: string }
  ): Promise<RunnerResponse> {
    const task = await this.todoAPI.getTask(args.id);

    const systemMessage = task
      ? [
          `Task details:`,
          `Id: ${task.id}`,
          `Title: ${task.title}`,
          `Description: ${task.description}`,
          `Due Date: ${task.dueDate}`,
          `Completed: ${task.completed}`,
        ]
      : ["Error, Task not found."];

    return {
      model: defaultModel,
      messages: [
        { role: "system", content: systemMessage.join("\n") },
        ...messages,
      ],
    };
  }
}
