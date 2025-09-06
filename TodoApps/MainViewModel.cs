using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace TodoApps
{
    public class MainViewModel : DependencyObject
    {
        private const string FirebaseApiKey = "AIzaSyCyaJKb18ibRtR8RSt4bRcopcy3XtuzU9k";
        private readonly FirebaseService firebase;
        private string IdToken;
        private string LocalUserId;
        private List<TodoTask> allTasks = new List<TodoTask>();
        private bool isDarkTheme = false;
        private TodoTask draggedTask;
        private DataGridRow targetRow;

        public MainViewModel()
        {
            firebase = new FirebaseService(FirebaseApiKey);
            LockUI();
        }

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(MainViewModel), new PropertyMetadata(""));
        public static readonly DependencyProperty StatusForegroundProperty =
            DependencyProperty.Register("StatusForeground", typeof(Brush), typeof(MainViewModel), new PropertyMetadata(Brushes.Red));
        public static readonly DependencyProperty TasksProperty =
            DependencyProperty.Register("Tasks", typeof(IEnumerable<TodoTask>), typeof(MainViewModel), new PropertyMetadata(null));
        public static readonly DependencyProperty IsTasksGridEnabledProperty =
            DependencyProperty.Register("IsTasksGridEnabled", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsAddTaskBtnEnabledProperty =
            DependencyProperty.Register("IsAddTaskBtnEnabled", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsEditTaskBtnEnabledProperty =
            DependencyProperty.Register("IsEditTaskBtnEnabled", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsDeleteTaskBtnEnabledProperty =
            DependencyProperty.Register("IsDeleteTaskBtnEnabled", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsToggleThemeBtnEnabledProperty =
            DependencyProperty.Register("IsToggleThemeBtnEnabled", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsFilterPanelEnabledProperty =
            DependencyProperty.Register("IsFilterPanelEnabled", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public Brush StatusForeground
        {
            get => (Brush)GetValue(StatusForegroundProperty);
            set => SetValue(StatusForegroundProperty, value);
        }

        public IEnumerable<TodoTask> Tasks
        {
            get => (IEnumerable<TodoTask>)GetValue(TasksProperty);
            set => SetValue(TasksProperty, value);
        }

        public bool IsTasksGridEnabled
        {
            get => (bool)GetValue(IsTasksGridEnabledProperty);
            set => SetValue(IsTasksGridEnabledProperty, value);
        }

        public bool IsAddTaskBtnEnabled
        {
            get => (bool)GetValue(IsAddTaskBtnEnabledProperty);
            set => SetValue(IsAddTaskBtnEnabledProperty, value);
        }

        public bool IsEditTaskBtnEnabled
        {
            get => (bool)GetValue(IsEditTaskBtnEnabledProperty);
            set => SetValue(IsEditTaskBtnEnabledProperty, value);
        }

        public bool IsDeleteTaskBtnEnabled
        {
            get => (bool)GetValue(IsDeleteTaskBtnEnabledProperty);
            set => SetValue(IsDeleteTaskBtnEnabledProperty, value);
        }

        public bool IsToggleThemeBtnEnabled
        {
            get => (bool)GetValue(IsToggleThemeBtnEnabledProperty);
            set => SetValue(IsToggleThemeBtnEnabledProperty, value);
        }

        public bool IsFilterPanelEnabled
        {
            get => (bool)GetValue(IsFilterPanelEnabledProperty);
            set => SetValue(IsFilterPanelEnabledProperty, value);
        }

        public async void SignIn(string email, string password)
        {
            try
            {
                var data = await firebase.SignInAsync(email, password);
                if (data == null)
                {
                    StatusText = "Ошибка входа!";
                    StatusForeground = Brushes.Red;
                    return;
                }
                IdToken = data.idToken;
                LocalUserId = data.localId;
                StatusText = "Вход успешен!";
                StatusForeground = Brushes.Green;
                UnlockUI();
                await LoadTasks();
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка авторизации!";
                StatusForeground = Brushes.Red;
                Console.WriteLine(ex.Message);
            }
        }

        public async void SignUp(string email, string password)
        {
            try
            {
                var success = await firebase.SignUpAsync(email, password);
                StatusText = success
                    ? "Регистрация успешна! Теперь войдите."
                    : "Ошибка регистрации!";
                StatusForeground = success ? Brushes.Green : Brushes.Red;
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка регистрации!";
                StatusForeground = Brushes.Red;
                Console.WriteLine(ex.Message);
            }
        }

        private async Task LoadTasks()
        {
            if (string.IsNullOrEmpty(LocalUserId)) return;
            try
            {
                var dict = await firebase.GetTasksAsync();
                if (dict != null)
                {
                    allTasks = dict.Select(x =>
                    {
                        x.Value.FirebaseId = x.Key;
                        return x.Value;
                    })
                    .Where(t => t.Category.StartsWith(LocalUserId + "|"))
                    .ToList();
                    using var db = new AppDbContext();
                    db.Database.EnsureCreated();
                    db.Tasks.RemoveRange(db.Tasks);
                    db.Tasks.AddRange(allTasks);
                    db.SaveChanges();
                }
                else
                {
                    using var db = new AppDbContext();
                    db.Database.EnsureCreated();
                    allTasks = db.Tasks.Where(t => t.Category.StartsWith(LocalUserId + "|")).ToList();
                }
            }
            catch
            {
                using var db = new AppDbContext();
                db.Database.EnsureCreated();
                allTasks = db.Tasks.Where(t => t.Category.StartsWith(LocalUserId + "|")).ToList();
            }
            Tasks = allTasks;
        }

        private void LockUI()
        {
            IsTasksGridEnabled = false;
            IsAddTaskBtnEnabled = false;
            IsEditTaskBtnEnabled = false;
            IsDeleteTaskBtnEnabled = false;
            IsToggleThemeBtnEnabled = false;
            IsFilterPanelEnabled = false;
            Tasks = null;
        }

        private void UnlockUI()
        {
            IsTasksGridEnabled = true;
            IsAddTaskBtnEnabled = true;
            IsEditTaskBtnEnabled = true;
            IsDeleteTaskBtnEnabled = true;
            IsToggleThemeBtnEnabled = true;
            IsFilterPanelEnabled = true;
        }

        public void FilterTasks(string searchText, string selectedCategory, bool? isCompleted)
        {
            if (string.IsNullOrEmpty(LocalUserId)) return;
            var filtered = allTasks.AsEnumerable();
            string search = searchText.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(t => t.Title.ToLower().Contains(search) || t.TagsString.ToLower().Contains(search));
            if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "Все")
                filtered = filtered.Where(t => t.Category.EndsWith(selectedCategory));
            if (isCompleted == true)
                filtered = filtered.Where(t => t.IsCompleted);
            Tasks = filtered.ToList();
        }

        public async void AddTask()
        {
            if (string.IsNullOrEmpty(LocalUserId)) return;
            var title = Interaction.InputBox("Название задачи:", "Добавление", "");
            if (string.IsNullOrWhiteSpace(title)) return;
            var description = Interaction.InputBox("Описание задачи:", "Добавление", "");
            var categoryInput = Interaction.InputBox("Категория:", "Добавление", "Без категории");
            var category = $"{LocalUserId}|{categoryInput}";
            var tagsStr = Interaction.InputBox("Теги (через запятую):", "Добавление", "");
            var recurrence = Interaction.InputBox("Повтор (None, Daily, Weekly, Monthly):", "Добавление", "None");
            DateTime? dueDate = null;
            var dueStr = Interaction.InputBox("Дедлайн (yyyy-MM-dd HH:mm):", "Добавление", "");
            if (!string.IsNullOrWhiteSpace(dueStr) && DateTime.TryParse(dueStr, out DateTime dt))
                dueDate = dt;
            var newTask = new TodoTask
            {
                Title = title,
                Description = description,
                Category = category,
                Tags = tagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList(),
                Recurrence = recurrence,
                DueDate = dueDate
            };
            newTask.FirebaseId = await firebase.AddTaskAsync(newTask);
            using var db = new AppDbContext();
            db.Tasks.Add(newTask);
            db.SaveChanges();
            await LoadTasks();
        }

        public async void EditTask(TodoTask selected)
        {
            if (string.IsNullOrEmpty(LocalUserId)) return;
            if (selected == null) return;
            var title = Interaction.InputBox("Название:", "Редактирование", selected.Title);
            var description = Interaction.InputBox("Описание:", "Редактирование", selected.Description ?? "");
            var categoryInput = Interaction.InputBox("Категория:", "Редактирование", selected.Category.Split('|')[1]);
            selected.Category = $"{LocalUserId}|{categoryInput}";
            var tagsStr = Interaction.InputBox("Теги:", "Редактирование", selected.TagsString);
            var recurrence = Interaction.InputBox("Повтор:", "Редактирование", selected.Recurrence);
            DateTime? dueDate = selected.DueDate;
            var dueStr = Interaction.InputBox("Дедлайн (yyyy-MM-dd HH:mm):", "Редактирование", selected.DueDate?.ToString("yyyy-MM-dd HH:mm") ?? "");
            if (!string.IsNullOrWhiteSpace(dueStr) && DateTime.TryParse(dueStr, out DateTime dt))
                dueDate = dt;
            selected.Title = title;
            selected.Description = description;
            selected.Tags = tagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            selected.Recurrence = recurrence;
            selected.DueDate = dueDate;
            await firebase.UpdateTaskAsync(selected);
            using var db = new AppDbContext();
            db.Tasks.Update(selected);
            db.SaveChanges();
            await LoadTasks();
        }

        public async void DeleteTask(TodoTask selected)
        {
            if (string.IsNullOrEmpty(LocalUserId)) return;
            if (selected == null) return;
            await firebase.DeleteTaskAsync(selected.FirebaseId);
            using var db = new AppDbContext();
            db.Tasks.Remove(selected);
            db.SaveChanges();
            await LoadTasks();
        }

        public void TasksGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e, DataGrid tasksGrid)
        {
            var row = ItemsControl.ContainerFromElement(tasksGrid, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null) draggedTask = row.Item as TodoTask;
        }

        public void TasksGrid_MouseMove(object sender, MouseEventArgs e, DataGrid tasksGrid)
        {
            if (e.LeftButton == MouseButtonState.Pressed && draggedTask != null)
                DragDrop.DoDragDrop(tasksGrid, draggedTask, DragDropEffects.Move);
        }

        public void TasksGrid_DragOver(object sender, DragEventArgs e, DataGrid tasksGrid)
        {
            var row = GetDataGridRowUnderMouse(e.GetPosition, tasksGrid);
            if (row != null && row != targetRow)
            {
                if (targetRow != null) targetRow.Background = Brushes.Transparent;
                targetRow = row;
                targetRow.Background = Brushes.LightGray;
            }
        }

        public void TasksGrid_Drop(object sender, DragEventArgs e, DataGrid tasksGrid)
        {
            if (draggedTask == null || targetRow == null) return;
            var targetTask = targetRow.Item as TodoTask;
            allTasks.Remove(draggedTask);
            var index = allTasks.IndexOf(targetTask);
            allTasks.Insert(index, draggedTask);
            Tasks = null;
            Tasks = allTasks;
            targetRow.Background = Brushes.Transparent;
            draggedTask = null;
            targetRow = null;
        }

        private DataGridRow GetDataGridRowUnderMouse(Func<IInputElement, Point> getPosition, DataGrid tasksGrid)
        {
            for (int i = 0; i < tasksGrid.Items.Count; i++)
            {
                var row = (DataGridRow)tasksGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row == null) continue;
                var bounds = VisualTreeHelper.GetDescendantBounds(row);
                var point = getPosition(row);
                if (bounds.Contains(point)) return row;
            }
            return null;
        }

        public void ToggleTheme(ResourceDictionary resources, DataGrid tasksGrid)
        {
            if (isDarkTheme)
            {
                resources["WindowBackground"] = new SolidColorBrush(Colors.White);
                resources["ForegroundBrush"] = new SolidColorBrush(Colors.Black);
                resources["DataGridBackground"] = new SolidColorBrush(Colors.White);
                resources["DataGridForeground"] = new SolidColorBrush(Colors.Black);
            }
            else
            {
                resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                resources["ForegroundBrush"] = new SolidColorBrush(Colors.White);
                resources["DataGridBackground"] = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                resources["DataGridForeground"] = new SolidColorBrush(Colors.White);
            }
            isDarkTheme = !isDarkTheme;
            tasksGrid.Items.Refresh();
        }
    }
}