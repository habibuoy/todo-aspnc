using Microsoft.EntityFrameworkCore;
using TodoApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("MainDb") ?? throw new InvalidOperationException("Connections string not set");
builder.Services.AddDbContext<TodoContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

var todo = app.MapGroup("/api/todos");

todo.MapGet("/", static async (TodoContext context) => await context.Todos.ToListAsync());

todo.MapGet("/{id}", static async (int id, TodoContext context) =>
{
    var todo = await context.Todos.FindAsync(id);
    if (todo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(todo);
});

todo.MapPost("/", static async (Todo input, TodoContext context) =>
{
    if (string.IsNullOrEmpty(input.Content))
    {
        return Results.BadRequest("Todo Content should not be empty"); ;
    }

    context.Todos.Add(input);
    await context.SaveChangesAsync();
    return Results.Created($"/{input.Id}", input);
});

todo.MapPost("/edit/{id}", static async (int id, TodoContext context, HttpContext httpContext, ILogger<Program> logger) =>
{
    Todo? input = null;

    if (httpContext.Request.ContentLength > 0)
    {
        try
        {
            input = await httpContext.Request.ReadFromJsonAsync<Todo>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Edit: Failed to deserialize request body");

            return Results.BadRequest("Please provide valid todo object");
        }
    }

    if (input == null
        || id != input.Id)
    {
        return Results.BadRequest("Please provide valid todo object");
    }

    var existing = await context.Todos.FindAsync(id);

    if (existing == null)
    {
        return Results.NotFound();
    }

    existing.Content = input.Content;
    existing.IsCompleted = input.IsCompleted;

    await context.SaveChangesAsync();

    return Results.Ok(existing);
});

todo.MapDelete("/delete/{id}", static async (int id, TodoContext context) =>
{
    var existing = await context.Todos.FindAsync(id);

    if (existing == null)
    {
        return Results.NotFound();
    }

    context.Todos.Remove(existing);

    await context.SaveChangesAsync();

    return Results.Ok();
});

app.Run();