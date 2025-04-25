using System.Diagnostics;
using System.Reflection;

namespace TodoApp;

public class TodoFilter
{
    public string? Content { get; set; }
    public bool? IsCompleted { get; set; }


    public static ValueTask<TodoFilter?> BindAsync(HttpContext httpContext, ParameterInfo parameterInfo)
    {
        if (httpContext.Request.Query.Count == 0)
        {
            return ValueTask.FromResult<TodoFilter?>(null);
        }

        string content = httpContext.Request.Query["content"]!;

        bool? isCompleted = null;
        if (bool.TryParse(httpContext.Request.Query["isCompleted"], out var completed))
        {
            isCompleted = completed;
        }

        var result = new TodoFilter()
        {
            Content = content,
            IsCompleted = isCompleted
        };

        return ValueTask.FromResult<TodoFilter?>(result);
    }
}