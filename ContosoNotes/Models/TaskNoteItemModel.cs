using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using ContosoNotes.Common;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ContosoNotes.Models
{
    public class TaskNoteItemModel : NoteItemModel
    {
        public bool IsCompleted { get; protected set; }

        public string TodoTaskId { get; set; }

        public string TodoTaskListId { get; set; }

        [JsonIgnore]
        public TodoTask TodoTask { get; protected set; }

        [JsonIgnore]
        public LoadingState LoadingState { get; protected set; }

        public TaskNoteItemModel(string text = "") : base(text)
        {
            LoadingState = LoadingState.Unloaded;
        }

        protected async void Load(bool force = false)
        {
            if (LoadingState == LoadingState.Loading)
            {
                return;
            }
            if (LoadingState == LoadingState.Loaded && !force)
            {
                return;
            }

            LoadingState = LoadingState.Loading;

            if (TodoTaskId == null || TodoTaskListId == null)
            {
                TodoTask = null;
                IsCompleted = false;
            }
            else
            {
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider != null && provider.State == ProviderState.SignedIn)
                {
                    var graph = ProviderManager.Instance.GlobalProvider.Graph();

                    try
                    {
                        // Retrieve the task.
                        TodoTask = await graph.Me.Todo.Lists[TodoTaskListId].Tasks[TodoTaskId].Request().GetAsync();
                        IsCompleted = TodoTask.Status == Microsoft.Graph.TaskStatus.Completed;
                    }
                    catch
                    {
                        // Task must not exist.
                        TodoTask = null;
                        IsCompleted = false;
                    }
                }
            }

            LoadingState = LoadingState.Loaded;
        }

        public async Task Save()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                var graph = ProviderManager.Instance.GlobalProvider.Graph();
                
                if (TodoTaskId == null)
                {
                    // Create a new task.
                    TodoTask newTask = await graph.Me.Todo.Lists[TodoTaskListId].Tasks.Request().AddAsync(new TodoTask()
                    {
                        Body = new ItemBody()
                        {
                            Content = Text
                        },
                    });

                    TodoTaskId = newTask.Id;
                }
                else
                {
                    // Update the existing task.
                    var taskForUpdate = new TodoTask()
                    {
                        Id = TodoTaskId,
                        Body = new ItemBody()
                        {
                            Content = Text,
                        },
                    };

                    if (IsCompleted)
                    {
                        taskForUpdate.Status = Microsoft.Graph.TaskStatus.Completed;
                    }

                    TodoTask = await graph.Me.Todo.Lists[TodoTaskListId].Tasks[TodoTaskId].Request().UpdateAsync(taskForUpdate);

                    IsCompleted = TodoTask.Status == Microsoft.Graph.TaskStatus.Completed;
                }
            }
        }
    }
}
