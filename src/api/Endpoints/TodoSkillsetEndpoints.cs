using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

public static class TodoSkillsetEndpoints
{
    /// <summary>
    /// Configures and maps the endpoints for managing Todo items in the Skillset module.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance used to register the endpoints.</param>
    /// <remarks>
    /// This method maps several POST endpoints to handle Todo operations:
    /// 
    /// 1. POST "/skillset/todos/list":
    ///    - Retrieves the authenticated user's GitHub token from the request headers.
    ///    - Uses <see cref="GitHubService"/> to obtain current user details.
    ///    - If a valid user is found, logs the user's ID, fetches the list of todos from <see cref="ITodoRepository"/>,
    ///      logs the count, and returns an HTTP 200 (OK) response with the todos.
    ///    - If the user ID is missing, logs the issue and returns an HTTP 400 (Bad Request).
    /// 
    /// 2. POST "/skillset/todos/add":
    ///    - Retrieves the GitHub token and resolves the current user.
    ///    - On a valid user, assigns the user's ID to the given Todo item, logs the addition, creates the Todo via the repository,
    ///      logs the success, and returns an HTTP 200 (OK) response.
    ///    - If the user is invalid, logs and returns an HTTP 400 (Bad Request).
    /// 
    /// 3. POST "/skillset/todos/update":
    ///    - Retrieves the GitHub token and resolves the current user.
    ///    - On a valid user, sets the user ID on the Todo item, logs the update attempt, and attempts to update the Todo via the repository.
    ///    - Returns an HTTP 200 (OK) with the updated Todo if successful, and logs the success; if the Todo is not found,
    ///      logs the issue and returns an HTTP 404 (Not Found).
    ///    - If the user is invalid, logs and returns an HTTP 400 (Bad Request).
    /// 
    /// 4. POST "/skillset/todos/delete":
    ///    - Retrieves the GitHub token and resolves the current user.
    ///    - Checks if the Todo exists and if it belongs to the authenticated user.
    ///    - Logs actions accordingly and deletes the Todo via the repository, returning an HTTP 200 (OK) on success.
    ///    - Returns an HTTP 404 (Not Found) if the Todo does not exist or an HTTP 400 (Bad Request) if the user is not authorized.
    /// </remarks>
    public static void MapTodoSkillsetEndpoints(this WebApplication app)
    {
        var logger = app.Logger;

        app.MapPost("/skillset/todos/list", async (HttpContext context, GitHubService gitHubService, ITodoRepository repository) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token);
            var userId = user?.Login;
            if (!string.IsNullOrEmpty(userId))
            {
                logger.LogInformation("Listing todos. UserId: {userId}", userId);
                var todos = await repository.ListTodosAsync(userId);
                logger.LogInformation("Returned {count} todos.", todos?.Count() ?? 0);
                return Results.Ok(todos);
            }
            else
            {
                logger.LogWarning("UserId is missing.");
                return Results.BadRequest("UserId is missing.");
            }
        })
        .WithName("GetSkillsetTodosList");

        app.MapPost("/skillset/todos/add", async ( ITodoRepository repository, TodoItem todo, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token);
            var userId = user?.Login;
            if (!string.IsNullOrEmpty(userId))
            {
                todo.UserId = userId;
                logger.LogInformation("Adding a new todo. UserId: {userId}", userId);
                await repository.CreateTodoAsync(todo);
                logger.LogInformation("Todo created successfully.");
                return Results.Ok();
            }
            else
            {
                logger.LogWarning("UserId is missing.");
                return Results.BadRequest("UserId is missing.");
            }
        })
        .WithName("SkillsetCreateTodo");

        app.MapPost("/skillset/todos/update", async (TodoItem todo, ITodoRepository repository,HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token);
            var userId = user?.Login;
            if (!string.IsNullOrEmpty(userId))
            {
                var task = await repository.GetTodoAsync(todo.Id);
                if (task == null)
                {
                    logger.LogWarning("Todo with id {id} not found for update.", todo.Id);
                    return Results.NotFound();
                }
                if (task.UserId != userId)
                {
                    logger.LogWarning("Todo with id {id} does not belong to user {userId}.", todo.Id, userId);
                    return Results.BadRequest();
                }
                todo.UserId = userId;
                todo.Title = todo.Title??task.Title;
                todo.Description = todo.Description??task.Description;
                todo.Completed = todo.Completed??task.Completed;
                todo.DueDate = todo.DueDate??task.DueDate;

                logger.LogInformation("Updating todo with id {id}.", todo.Id);
                var updatedTodo = await repository.UpdateTodoAsync(todo);
                if (updatedTodo == null)
                {
                    logger.LogWarning("Todo with id {id} not found for update.", todo.Id);
                    return Results.NotFound();
                }

                logger.LogInformation("Todo updated successfully.");
                return Results.Ok(updatedTodo);
            }
            else
            {
                logger.LogWarning("UserId is missing.");
                return Results.BadRequest("UserId is missing.");
            }
        })
        .WithName("SkillsetUpdateTodo");

        app.MapPost("/skillset/todos/delete", async (string id, ITodoRepository repository,HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token);
            var userId = user?.Login;
            if (!string.IsNullOrEmpty(userId))
            {
                var task = await repository.GetTodoAsync(id);
                if (task == null)
                {
                    logger.LogWarning("Todo with id {id} not found for deletion.", id);
                    return Results.NotFound();
                }
                if (task.UserId != userId)
                {
                    logger.LogWarning("Todo with id {id} does not belong to user {userId}.", id, userId);
                    return Results.BadRequest();
                }
                logger.LogInformation("Deleting todo with id {id}.", id);
                var success = await repository.DeleteTodoAsync(id);
                if (!success)
                {
                    logger.LogWarning("Todo with id {id} not found for deletion.", id);
                    return Results.NotFound();
                }
                logger.LogInformation("Todo deleted successfully.");
                return Results.Ok();
            }
            else
            {
                logger.LogWarning("UserId is missing.");
                return Results.BadRequest("UserId is missing.");
            }
        })
        .WithName("SkillsetDeleteTodo");
    }
}