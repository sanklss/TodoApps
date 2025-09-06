using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TodoApps
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;
            StatusText.SetBinding(TextBlock.TextProperty, new Binding("StatusText") { Source = viewModel });
            StatusText.SetBinding(TextBlock.ForegroundProperty, new Binding("StatusForeground") { Source = viewModel });
            TasksGrid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Tasks") { Source = viewModel });
            TasksGrid.SetBinding(UIElement.IsEnabledProperty, new Binding("IsTasksGridEnabled") { Source = viewModel });
            AddTaskBtn.SetBinding(UIElement.IsEnabledProperty, new Binding("IsAddTaskBtnEnabled") { Source = viewModel });
            EditTaskBtn.SetBinding(UIElement.IsEnabledProperty, new Binding("IsEditTaskBtnEnabled") { Source = viewModel });
            DeleteTaskBtn.SetBinding(UIElement.IsEnabledProperty, new Binding("IsDeleteTaskBtnEnabled") { Source = viewModel });
            ToggleThemeBtn.SetBinding(UIElement.IsEnabledProperty, new Binding("IsToggleThemeBtnEnabled") { Source = viewModel });
            FilterPanel.SetBinding(UIElement.IsEnabledProperty, new Binding("IsFilterPanelEnabled") { Source = viewModel });
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SignIn(EmailBox.Text, PasswordBox.Password);
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SignUp(EmailBox.Text, PasswordBox.Password);
        }

        private void FilterTasks(object sender, System.EventArgs e)
        {
            string selectedCategory = CategoryFilter.SelectedItem is ComboBoxItem ci ? ci.Content.ToString() : "Все";
            viewModel.FilterTasks(SearchBox.Text, selectedCategory, CompletedFilter.IsChecked);
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddTask();
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            viewModel.EditTask(TasksGrid.SelectedItem as TodoTask);
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DeleteTask(TasksGrid.SelectedItem as TodoTask);
        }

        private void TasksGrid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            viewModel.TasksGrid_PreviewMouseLeftButtonDown(sender, e, TasksGrid);
        }

        private void TasksGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            viewModel.TasksGrid_MouseMove(sender, e, TasksGrid);
        }

        private void TasksGrid_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            viewModel.TasksGrid_DragOver(sender, e, TasksGrid);
        }

        private void TasksGrid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            viewModel.TasksGrid_Drop(sender, e, TasksGrid);
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ToggleTheme(Resources, TasksGrid);
        }
    }
}