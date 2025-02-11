using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public InMemoryTodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TodoItem>> ListTodosAsync(string? userId)
    {
        return await _context.TodoItems
            .Where(t => userId == null || t.UserId == userId)
            .ToListAsync();
    }

    public async Task<TodoItem?> GetTodoAsync(string id)
    {
        return await _context.TodoItems.FindAsync(id);
    }

    public async Task CreateTodoAsync(TodoItem todo)
    {
        todo.Id = Guid.NewGuid().ToString();
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();
    }

    public async Task<TodoItem?> UpdateTodoAsync(TodoItem todo)
    {
        var existingTodo = await _context.TodoItems.FindAsync(todo.Id);
        if (existingTodo == null)
        {
            return null;
        }

        _context.Entry(existingTodo).CurrentValues.SetValues(todo);
        await _context.SaveChangesAsync();
        return existingTodo;
    }

    public async Task<bool> DeleteTodoAsync(string id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
        {
            return false;
        }

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems { get; set; }
}