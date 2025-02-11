using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITodoRepository
{
    public Task<IEnumerable<TodoItem>> ListTodosAsync(string? userId);
    
    public Task<TodoItem?> GetTodoAsync(string id);
    public Task CreateTodoAsync(TodoItem todo);
    public Task<TodoItem?> UpdateTodoAsync(TodoItem todo);
    public Task<bool> DeleteTodoAsync(string id);


}