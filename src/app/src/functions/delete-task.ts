import OpenAI from "openai";
import { RunnerResponse, defaultModel, Tool } from "../functions.js";
import { Task } from "../todo-api.js";
import { randomUUID } from "crypto";

export class deleteTask extends Tool {
  static definition = {
    name: "delete_task",
    description: "This function delete a task for the current user.",
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
    args: { id: string}
  ): Promise<RunnerResponse> {

    const done = await this.todoAPI.deleteTask(args.id);
        

    const systemMessage = done 
      ? ["Task deleted successfully."]
      : ["Error, Task not deleted."];

    return {
      model: defaultModel,
      messages: [
        { role: "system", content: systemMessage.join("\n") },
        ...messages,
      ],
    };
  }
}
