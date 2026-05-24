using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TaskHub
{
    public enum Priority { Low, Medium, High }
    public enum Status { New, InProgress, Done }

    public delegate bool TaskFilter(TaskItem task);

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public override string ToString()
        {
            string deadlineStr = Deadline.HasValue ? Deadline.Value.ToString("dd.MM.yyyy HH:mm") : "None";
            return $"Id: {Id} | {Title} | Priority: {Priority} | Status: {Status} | Deadline: {deadlineStr}";
        }
    }

    public class TaskManager
    {
        private readonly List<TaskItem> _tasks = new List<TaskItem>();
        private int _nextId = 1;
        private readonly object _lock = new object();

        public void AddTask(string title, string description, Priority priority, DateTime? deadline)
        {
            lock (_lock)
            {
                var task = new TaskItem
                {
                    Id = _nextId++,
                    Title = title,
                    Description = description,
                    Priority = priority,
                    Deadline = deadline,
                    Status = Status.New
                };
                _tasks.Add(task);
            }
        }

        public List<TaskItem> GetAllTasks()
        {
            lock (_lock) return _tasks.ToList();
        }

        public List<TaskItem> GetCompleted()
        {
            lock (_lock) return _tasks.Where(t => t.Status == Status.Done).ToList();
        }

        public List<TaskItem> GetNotCompleted()
        {
            lock (_lock) return _tasks.Where(t => t.Status != Status.Done).ToList();
        }

        public List<TaskItem> GetHighPriority()
        {
            lock (_lock) return _tasks.Where(t => t.Priority == Priority.High).ToList();
        }

        public bool UpdateTask(int id, string title, string description, Priority? priority, Status? status)
        {
            lock (_lock)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == id);
                if (task == null) return false;
                if (title != null) task.Title = title;
                if (description != null) task.Description = description;
                if (priority.HasValue) task.Priority = priority.Value;
                if (status.HasValue) task.Status = status.Value;
                return true;
            }
        }

        public bool DeleteTask(int id)
        {
            lock (_lock)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == id);
                if (task == null) return false;
                _tasks.Remove(task);
                return true;
            }
        }

        public List<TaskItem> Search(TaskFilter filter)
        {
            lock (_lock) return _tasks.Where(t => filter(t)).ToList();
        }

        public void ShowStatistics()
        {
            lock (_lock)
            {
                int total = _tasks.Count;
                int done = _tasks.Count(t => t.Status == Status.Done);
                int overdue = _tasks.Count(t => t.Status != Status.Done
                                               && t.Deadline.HasValue
                                               && t.Deadline.Value < DateTime.Now);
                var byPriority = new Dictionary<Priority, int>();
                foreach (Priority p in Enum.GetValues(typeof(Priority)))
                    byPriority[p] = _tasks.Count(t => t.Priority == p);

                Console.WriteLine($"Total tasks: {total}");
                Console.WriteLine($"Completed: {done}");
                Console.WriteLine($"Overdue: {overdue}");
                Console.WriteLine("By priority:");
                foreach (var kvp in byPriority)
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        public List<TaskItem> GetOverdueTasks()
        {
            lock (_lock)
            {
                return _tasks.Where(t => t.Status != Status.Done
                                       && t.Deadline.HasValue
                                       && t.Deadline.Value < DateTime.Now).ToList();
            }
        }

        public List<TaskItem> Tasks
        {
            get { lock (_lock) return _tasks.ToList(); }
            set
            {
                lock (_lock)
                {
                    _tasks.Clear();
                    _tasks.AddRange(value);
                    if (_tasks.Any())
                        _nextId = _tasks.Max(t => t.Id) + 1;
                }
            }
        }
    }

    public class DeadlineChecker : IDisposable
    {
        private readonly TaskManager _manager;
        private CancellationTokenSource _cts;
        private Task _backgroundTask;
        public event Action<string> OnOverdueNotification;

        public DeadlineChecker(TaskManager manager)
        {
            _manager = manager;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _backgroundTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var overdue = _manager.GetOverdueTasks();
                    foreach (var task in overdue)
                    {
                        OnOverdueNotification?.Invoke($"WARNING: task \"{task.Title}\" is overdue! (deadline {task.Deadline})");
                    }
                    try
                    {
                        await Task.Delay(5000, _cts.Token);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            try { _backgroundTask?.Wait(1000); } catch { }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }

    public static class FileManager
    {
        public static async Task SaveAsync(string path, List<TaskItem> tasks)
        {
            try
            {
                var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(path, json);
                Console.WriteLine("Tasks saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save error: {ex.Message}");
            }
        }

        public static async Task<List<TaskItem>> LoadAsync(string path)
        {
            try
            {
                if (!File.Exists(path)) return new List<TaskItem>();
                var json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
                return new List<TaskItem>();
            }
        }
    }

    class Program
    {
        static TaskManager manager = new TaskManager();
        static DeadlineChecker checker;

        static async Task Main(string[] args)
        {
            checker = new DeadlineChecker(manager);
            checker.OnOverdueNotification += msg => Console.WriteLine(msg);
            checker.Start();

            var loaded = await FileManager.LoadAsync("tasks.json");
            manager.Tasks = loaded;

            while (true)
            {
                Console.WriteLine("\n=== TaskHub ===");
                Console.WriteLine("1. Create task");
                Console.WriteLine("2. Show all tasks");
                Console.WriteLine("3. Completed");
                Console.WriteLine("4. Not completed");
                Console.WriteLine("5. High priority tasks");
                Console.WriteLine("6. Edit task");
                Console.WriteLine("7. Delete task");
                Console.WriteLine("8. Search tasks");
                Console.WriteLine("9. Statistics");
                Console.WriteLine("0. Exit");
                Console.Write("Choice: ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": CreateTask(); break;
                        case "2": ShowTasks(manager.GetAllTasks()); break;
                        case "3": ShowTasks(manager.GetCompleted()); break;
                        case "4": ShowTasks(manager.GetNotCompleted()); break;
                        case "5": ShowTasks(manager.GetHighPriority()); break;
                        case "6": EditTask(); break;
                        case "7": DeleteTask(); break;
                        case "8": SearchTasks(); break;
                        case "9": manager.ShowStatistics(); break;
                        case "0":
                            await FileManager.SaveAsync("tasks.json", manager.Tasks);
                            checker.Stop();
                            checker.Dispose();
                            return;
                        default: Console.WriteLine("Invalid choice."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void ShowTasks(List<TaskItem> tasks)
        {
            if (tasks.Count == 0) { Console.WriteLine("No tasks."); return; }
            foreach (var t in tasks)
                Console.WriteLine(t);
        }

        static void CreateTask()
        {
            Console.Write("Title: ");
            string title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Title cannot be empty.");
                return;
            }

            Console.Write("Description: ");
            string desc = Console.ReadLine();

            Console.Write("Priority (Low/Medium/High): ");
            if (!Enum.TryParse(Console.ReadLine(), true, out Priority priority))
            { Console.WriteLine("Invalid priority. Set to Low."); priority = Priority.Low; }

            Console.Write("Deadline (dd.MM.yyyy HH:mm or empty): ");
            string deadInput = Console.ReadLine();
            DateTime? deadline = null;
            if (!string.IsNullOrWhiteSpace(deadInput) && DateTime.TryParse(deadInput, out DateTime dt))
                deadline = dt;

            manager.AddTask(title, desc, priority, deadline);
            Console.WriteLine("Task created.");
        }

        static void EditTask()
        {
            Console.Write("Task Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid Id."); return; }
            Console.Write("New title (Enter - no change): ");
            string title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title)) title = null;
            Console.Write("New description (Enter - no change): ");
            string desc = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(desc)) desc = null;
            Console.Write("New priority (Low/Medium/High, Enter - no change): ");
            string prStr = Console.ReadLine();
            Priority? priority = null;
            if (!string.IsNullOrWhiteSpace(prStr) && Enum.TryParse(prStr, true, out Priority pr))
                priority = pr;
            Console.Write("New status (New/InProgress/Done, Enter - no change): ");
            string stStr = Console.ReadLine();
            Status? status = null;
            if (!string.IsNullOrWhiteSpace(stStr) && Enum.TryParse(stStr, true, out Status st))
                status = st;

            if (manager.UpdateTask(id, title, desc, priority, status))
                Console.WriteLine("Task updated.");
            else
                Console.WriteLine("Task not found.");
        }

        static void DeleteTask()
        {
            Console.Write("Task Id: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid Id."); return; }
            if (manager.DeleteTask(id))
                Console.WriteLine("Task deleted.");
            else
                Console.WriteLine("Task not found.");
        }

        static void SearchTasks()
        {
            Console.WriteLine("Search by: 1 - title, 2 - status, 3 - priority");
            string opt = Console.ReadLine();
            List<TaskItem> results = null;
            switch (opt)
            {
                case "1":
                    Console.Write("Enter part of title: ");
                    string part = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(part))
                    {
                        Console.WriteLine("Search string cannot be empty.");
                        return;
                    }
                    part = part.ToLower();
                    results = manager.Search(t => t.Title.ToLower().Contains(part));
                    break;
                case "2":
                    Console.Write("Status (New/InProgress/Done): ");
                    if (Enum.TryParse(Console.ReadLine(), true, out Status st))
                        results = manager.Search(t => t.Status == st);
                    else Console.WriteLine("Invalid status.");
                    break;
                case "3":
                    Console.Write("Priority (Low/Medium/High): ");
                    if (Enum.TryParse(Console.ReadLine(), true, out Priority pr))
                        results = manager.Search(t => t.Priority == pr);
                    else Console.WriteLine("Invalid priority.");
                    break;
                default: Console.WriteLine("Invalid option."); break;
            }
            if (results != null)
                ShowTasks(results);
        }
    }
}