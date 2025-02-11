using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

public class CosmosTodoRepository : ITodoRepository
{
    private readonly Container _container;
    private readonly CosmosClient _cosmosClient;

    public CosmosTodoRepository(CosmosClient cosmosClient, IConfiguration configuration)
    {
        _cosmosClient = cosmosClient;

        const string DefaultDatabaseId = "TodoDb";
        const string DefaultContainerId = "Todos";
        
        string databaseId = configuration.GetValue<string>("COSMOS_DB") ?? DefaultDatabaseId;
        string containerId = configuration.GetValue<string>("COSMOS_CONTAINER") ?? DefaultContainerId;

        _container = _cosmosClient.GetContainer(databaseId, containerId);

    }

    public async Task<IEnumerable<TodoItem>> ListTodosAsync(string? userId)
    {
        string sqlQueryText = string.IsNullOrWhiteSpace(userId)
            ? "SELECT * FROM c"
            : "SELECT * FROM c WHERE c.userId = @userId";

        QueryDefinition queryDefinition = string.IsNullOrWhiteSpace(userId)
            ? new QueryDefinition(sqlQueryText)
            : new QueryDefinition(sqlQueryText).WithParameter("@userId", userId);

        var query = _container.GetItemQueryIterator<TodoItem>(queryDefinition);
        var results = new List<TodoItem>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.Resource);
        }
        return results;
    }


    public async Task CreateTodoAsync(TodoItem todo)
    {
        todo.Id = Guid.NewGuid().ToString();
        await _container.CreateItemAsync(todo, new PartitionKey(todo.Id));
    }

    public async Task<TodoItem?> UpdateTodoAsync(TodoItem todo)
    {
        try
        {
            var response = await _container.ReplaceItemAsync(todo, todo.Id, new PartitionKey(todo.Id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteTodoAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<TodoItem>(id, new PartitionKey(id));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<TodoItem?> GetTodoAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<TodoItem>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}