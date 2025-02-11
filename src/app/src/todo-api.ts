import axios from "axios";

export interface Task {
  id: string;
  userId: string;
  title: string;
  description: string;
  dueDate?: string;
  completed?: boolean;
}

export class TodoAPI {
  
  private _tasks: Task[] | null = null;
  private token: string;
  private baseUrl: string;

  constructor(baseUrl: string,token: string) {
    this.baseUrl = baseUrl;
    this.token = token;
  }

  async getTask(id: string): Promise<Task> {
    const { data: task } = await axios.get<Task>(`${this.baseUrl}/api/todos/${id}`, {
      headers: {
      Authorization: `Bearer ${this.token}`,
      "x-github-token": this.token
      }
    });
    return task;
  }

  async getTasks(): Promise<Task[]> {
    const { data: tasks } = await axios.get<Task[]>(`${this.baseUrl}/api/todos`, {
      headers: {
        Authorization: `Bearer ${this.token}`,
        "x-github-token": this.token
        }
    });
    return tasks;
  }

  async createTask(task: Task): Promise<Task> {
    const { data: newTask } = await axios.post<Task>(`${this.baseUrl}/api/todos`, task, {
      headers: {
        Authorization: `Bearer ${this.token}`,
        "x-github-token": this.token
        }
    });
    return newTask;
  }

  async updateTask(task: Task): Promise<Task> {
    const { data: updatedTask } = await axios.put<Task>(`${this.baseUrl}/api/todos/${task.id}`, task, {
      headers: {
        Authorization: `Bearer ${this.token}`,
        "x-github-token": this.token
        }
    });
    return updatedTask;
  }

  async deleteTask(id: string): Promise<boolean> {
    var result = await axios.delete(`${this.baseUrl}/api/todos/${id}`, {
      headers: {
        Authorization: `Bearer ${this.token}`,
        "x-github-token": this.token
        }
    });
    if (result.status != 200) {
      return false;
    }
    return true;
  }


}
