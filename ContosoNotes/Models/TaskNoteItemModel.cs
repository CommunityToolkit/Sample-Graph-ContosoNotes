// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using ContosoNotes.Common;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading;
using Windows.System;

namespace ContosoNotes.Models
{
    public class TaskNoteItemModel : NoteItemModel
    {
        private static SemaphoreSlim _mutex = new SemaphoreSlim(1);

        public string TodoTaskId { get; set; }

        public string TodoTaskListId { get; set; }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        [JsonIgnore]
        public TodoTask TodoTask { get; protected set; }

        [JsonIgnore]
        public LoadingState LoadingState { get; protected set; }

        private DispatcherQueueTimer _timer;

        public TaskNoteItemModel()
        {
            LoadingState = LoadingState.Unloaded;
            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();

            Load();
        }

        public TaskNoteItemModel(string text = null) : base(text)
        {
            LoadingState = LoadingState.Unloaded;
            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();

            Load();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsCompleted):
                case nameof(Text):
                    _timer.Debounce(() => Save(), TimeSpan.FromSeconds(3));
                    break;
            }

            base.OnPropertyChanged(e);
        }

        protected async void Load(bool force = false)
        {
            await _mutex.WaitAsync();

            try
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
                        var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();

                        try
                        {
                            // Retrieve the task.
                            TodoTask = await graphClient.Me.Todo.Lists[TodoTaskListId].Tasks[TodoTaskId].Request().GetAsync();
                            IsCompleted = TodoTask.Status == TaskStatus.Completed;
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
            catch
            {
                LoadingState = LoadingState.Unloaded;
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async void Save()
        {
            await _mutex.WaitAsync();

            try
            {
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider != null && provider.State == ProviderState.SignedIn)
                {
                    var graph = ProviderManager.Instance.GlobalProvider.GetClient();

                    if (TodoTaskListId == null)
                    {
                        try
                        {
                            var existingLists = await graph.Me.Todo.Lists.Request().Filter("displayName eq 'ContosoNotes'").GetAsync();
                            if (existingLists.Count == 0)
                            {
                                TodoTaskList newTaskList = await graph.Me.Todo.Lists.Request().AddAsync(new TodoTaskList()
                                {
                                    ODataType = null, // Magic, don't remove
                                    DisplayName = "ContosoNotes"
                                });

                                TodoTaskListId = newTaskList.Id;
                            }
                            else
                            {
                                TodoTaskListId = existingLists[0].Id;
                            }
                        }
                        catch
                        {
                            // Unable to retrieve or create the TodoTaskList. Bail.
                            return;
                        }
                    }

                    if (TodoTaskId == null)
                    {
                        // Create a new task.
                        TodoTask newTask = await graph.Me.Todo.Lists[TodoTaskListId].Tasks.Request().AddAsync(new TodoTask()
                        {
                            ODataType = null, // Magic, don't remove
                            Title = Text,
                        });

                        TodoTask = newTask;
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
                            Status = IsCompleted ? TaskStatus.Completed : TaskStatus.NotStarted
                        };

                        TodoTask = await graph.Me.Todo.Lists[TodoTaskListId].Tasks[TodoTaskId].Request().UpdateAsync(taskForUpdate);
                    }

                    IsCompleted = TodoTask.Status == TaskStatus.Completed;
                }
            }
            catch
            {
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async void Delete()
        {
            await _mutex.WaitAsync();

            try
            {
                if (TodoTaskListId == null || TodoTaskId == null)
                {
                    return;
                }

                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider != null && provider.State == ProviderState.SignedIn)
                {
                    var graph = ProviderManager.Instance.GlobalProvider.GetClient();

                    await graph.Me.Todo.Lists[TodoTaskListId].Tasks[TodoTaskId].Request().DeleteAsync();
                }
            }
            catch
            {
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
