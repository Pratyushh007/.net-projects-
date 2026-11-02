using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public enum Priority
{
    Low,
    Medium,
    High
}

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Priority Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        string status = IsCompleted ? "[X]" : "[ ]";
        return $"{Id}. {status} {Title} | Priority: {Priority} | Created: {CreatedAt:yyyy-MM-dd}";
    }
}

public class StorageService
{
    private readonly string _filePath;

    public StorageService(string filePath = "todos.json")
    {
        _filePath = filePath;
    }

    public List<TodoItem> Load()
    {
        if (!File.Exists(_filePath))
            return new List<TodoItem>();

        string json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new List<TodoItem>();
    }

    public void Save(List<TodoItem> items)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(items, options);
        File.WriteAllText(_filePath, json);
    }
}

public class TaskManager
{
    private readonly List<TodoItem> _tasks;
    private readonly StorageService _storage;

    public TaskManager(StorageService storage)
    {
        _storage = storage;
        _tasks = _storage.Load();
    }

    public void AddTask(string title, string description, Priority priority)
    {

        int nextId = _tasks.Any() ? _tasks.Max(t => t.Id) + 1 : 1;

        _tasks.Add(new TodoItem
        {
            Id = nextId,
            Title = title,
            Description = description,
            Priority = priority
        });

        _storage.Save(_tasks);
    }

    public bool MarkComplete(int id)
    {

        var item = _tasks.FirstOrDefault(t => t.Id == id);
        if (item == null) return false;

        item.IsCompleted = true;
        _storage.Save(_tasks);
        return true;
    }

    public bool DeleteTask(int id)
    {
        var item = _tasks.FirstOrDefault(t => t.Id == id);
        if (item == null) return false;

        _tasks.Remove(item);
        _storage.Save(_tasks);
        return true;
    }

    public IEnumerable<TodoItem> GetPendingTasks() =>
        _tasks.Where(t => !t.IsCompleted).OrderBy(t => t.Priority);

    public IEnumerable<TodoItem> GetCompletedTasks() =>
        _tasks.Where(t => t.IsCompleted).OrderByDescending(t => t.CreatedAt);

    public IEnumerable<TodoItem> Search(string keyword) =>
        _tasks.Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    public (int Total, int Pending, int HighPriority) GetStats() =>
        (_tasks.Count,
         _tasks.Count(t => !t.IsCompleted),
         _tasks.Count(t => !t.IsCompleted && t.Priority == Priority.High));
}

class Program
{
    static void Main()
    {
        var storage = new StorageService("todos.json");
        var manager = new TaskManager(storage);

        while (true)
        {
            Console.Clear();
            var (total, pending, urgent) = manager.GetStats();
            Console.WriteLine($"=== TO-DO APP | Total: {total} | Pending: {pending} | Urgent: {urgent} ===");
            Console.WriteLine("1. View Pending Tasks");
            Console.WriteLine("2. View Completed Tasks");
            Console.WriteLine("3. Add Task");
            Console.WriteLine("4. Mark Task Complete");
            Console.WriteLine("5. Delete Task");
            Console.WriteLine("6. Search Tasks");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("\n--- Pending Tasks ---");
                    foreach (var task in manager.GetPendingTasks())
                        Console.WriteLine(task);
                    break;

                case "2":
                    Console.WriteLine("\n--- Completed Tasks ---");
                    foreach (var task in manager.GetCompletedTasks())
                        Console.WriteLine(task);
                    break;

                case "3":
                    Console.Write("Title: ");
                    string title = Console.ReadLine() ?? "";
                    Console.Write("Description: ");
                    string desc = Console.ReadLine() ?? "";
                    Console.Write("Priority (0: Low, 1: Medium, 2: High): ");
                    Enum.TryParse(Console.ReadLine(), out Priority priority);

                    manager.AddTask(title, desc, priority);
                    Console.WriteLine("Task created!");
                    break;

                case "4":
                    Console.Write("Enter Task ID to complete: ");
                    if (int.TryParse(Console.ReadLine(), out int completeId))
                        Console.WriteLine(manager.MarkComplete(completeId) ? "Marked complete!" : "Not found.");
                    break;

                case "5":
                    Console.Write("Enter Task ID to delete: ");
                    if (int.TryParse(Console.ReadLine(), out int deleteId))
                        Console.WriteLine(manager.DeleteTask(deleteId) ? "Task removed!" : "Not found.");
                    break;

                case "6":
                    Console.Write("Enter search term: ");
                    string query = Console.ReadLine() ?? "";
                    foreach (var match in manager.Search(query))
                        Console.WriteLine(match);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress Enter to return to menu...");
            Console.ReadLine();
        }
    }
}
