namespace TodoApp;

public class Todo
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public bool IsCompleted { get; set; }
}