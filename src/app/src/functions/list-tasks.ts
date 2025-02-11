import OpenAI from "openai";
import { RunnerResponse, defaultModel, Tool } from "../functions.js";

export class listTasks extends Tool {
  static definition = {
    name: "list_tasks",
    description:
      "This function lists the tasks for the current user.",
    parameters: {
      type: "object",
      properties: {},
      description:
        "This function does not require any input parameters. It simply returns a list of tasks.",
    },
  };

  async execute(
    messages: OpenAI.ChatCompletionMessageParam[]
  ): Promise<RunnerResponse> {
    const tasks = await this.todoAPI.getTasks();

    const systemMessage = [
      "The user is asking for a list of tasks.",
      "Respond with a concise and readable list of the tasks, with a short description for each one.",
      "Use markdown formatting to make each description more readable.",
      "Begin each task's description with a header consisting of the task Title and id",
      "That list of tasks is as follows:",
      JSON.stringify(
        tasks.map((task) => ({
            id: task.id,
            title: task.title,
            description: task.description,
            dueDate: task.dueDate,
            completed: task.completed,

        }))
      ),
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
