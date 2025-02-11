using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var logger = app.Logger;

        app.MapGet("/api/todos", async (ITodoRepository repository, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
                context.Request.Headers.TryGetValue("x-github-token", out var token);
                var user = await gitHubService.GetCurrentUserAsync(token!);
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
        .WithName("GetTodosList");

        app.MapGet("/api/todos/{id}", async (string id, ITodoRepository repository, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
                context.Request.Headers.TryGetValue("x-github-token", out var token);
                var user = await gitHubService.GetCurrentUserAsync(token!);
                var userId = user?.Login;
                if (!string.IsNullOrEmpty(userId))
                {
                    logger.LogInformation("Listing todos. UserId: {userId}", userId);
                    var todo = await repository.GetTodoAsync(id);
                    if (todo == null)
                    {
                        logger.LogWarning("Todo with id {id} not found.", id);
                        return Results.NotFound();
                    }
                    if (todo.UserId != userId)
                    {
                        logger.LogWarning("User {userId} is not authorized to view todo with id {id}.", userId, id);
                        return Results.BadRequest();
                    }

                    return Results.Ok(todo);
                }
                else
                {
                    logger.LogWarning("UserId is missing.");
                    return Results.BadRequest("UserId is missing.");
                }
        })
                .WithName("GetTodo");

        app.MapPost("/api/todos", async (TodoItem todo, ITodoRepository repository, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token!);
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
        .WithName("CreateTodo");

        _ = app.MapPut("/api/todos/{id}", async (string id, TodoItem todo, ITodoRepository repository, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token!);
            var userId = user?.Login;
            if (!string.IsNullOrEmpty(userId))
            {
                var task = await repository.GetTodoAsync(id);
                if (task == null)
                {
                    logger.LogWarning("Todo with id {id} not found for update.", id);
                    return Results.NotFound();
                }
                if (task.UserId != userId)
                {
                    logger.LogWarning("User {userId} is not authorized to update todo with id {id}.", userId, id);
                    return Results.BadRequest();
                }
                todo.UserId = userId;
                logger.LogInformation("Updating todo with id {id}.", id);
                var updatedTodo = await repository.UpdateTodoAsync(todo);
                if (updatedTodo == null)
                {
                    logger.LogWarning("Todo with id {id} not found for update.", id);
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
        .WithName("UpdateTodo");

        app.MapDelete("/api/todos/{id}", async (string id, ITodoRepository repository, HttpContext context, RequestValidationService requestValidationService, GitHubService gitHubService) =>
        {
            context.Request.Headers.TryGetValue("x-github-token", out var token);
            var user = await gitHubService.GetCurrentUserAsync(token!);
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
                    logger.LogWarning("User {userId} is not authorized to delete todo with id {id}.", userId, id);
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
        .WithName("DeleteTodo");
    }
}