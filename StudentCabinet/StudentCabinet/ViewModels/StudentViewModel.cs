using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using StudentCabinet.Models;
using StudentCabinet.Services;

namespace StudentCabinet.ViewModels
{
    public class StudentViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly IAppStateService _appState =
            IPlatformApplication.Current?.Services.GetService<IAppStateService>()
            ?? new AppStateService();

        private readonly IStudentRepository _repository =
      IPlatformApplication.Current?.Services.GetService<IStudentRepository>()
      ?? new StudentRepository(Path.Combine(FileSystem.AppDataDirectory, "students.db3"),
          new SyncService(Path.Combine(FileSystem.AppDataDirectory, "students.db3"),
              Microsoft.Extensions.Logging.Abstractions.NullLogger<SyncService>.Instance),
          Microsoft.Extensions.Logging.Abstractions.NullLogger<StudentRepository>.Instance);

        private readonly Student _student = new();

        public string FullName
        {
            get => _student.FullName;
            set
            {
                if (_student.FullName != value)
                {
                    _student.FullName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Greeting));
                    (AddStudentCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public string Group
        {
            get => _student.Group;
            set
            {
                if (_student.Group != value)
                {
                    _student.Group = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Greeting));
                }
            }
        }

        public double AverageScore
        {
            get => _student.AverageScore;
            set
            {
                if (_student.AverageScore != value)
                {
                    _student.AverageScore = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AverageScoreColor));
                }
            }
        }

        public string Greeting => $"Студент: {FullName}, група {Group}";

        public Color AverageScoreColor => AverageScore >= 4.0 ? Colors.SeaGreen : Colors.IndianRed;

        public ObservableCollection<Student> Students { get; } = new();

        public ICommand AddStudentCommand { get; }
        public ICommand OpenDetailsCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand LoadCommand { get; }

        public StudentViewModel()
        {
            AddStudentCommand = new Command(async () => await AddStudentAsync(), CanAddStudent);
            OpenDetailsCommand = new Command<Student>(async (student) => await OpenDetailsAsync(student));
            DeleteStudentCommand = new Command<Student>(async (student) => await DeleteStudentAsync(student));
            LoadCommand = new Command(async () => await LoadAsync());
        }

        private async Task LoadAsync()
        {
            System.Diagnostics.Debug.WriteLine("[Lifecycle] StudentViewModel.LoadAsync()");

            Students.Clear();
            foreach (var s in await _repository.GetAllAsync())
                Students.Add(s);

            if (_appState.PendingDeepLinkStudentId is int pendingId)
            {
                _appState.PendingDeepLinkStudentId = null;
                var target = Students.FirstOrDefault(s => s.Id == pendingId);
                if (target is not null)
                    _ = OpenDeepLinkedStudentAsync(target);
            }
        }

        private async Task AddStudentAsync()
        {
            var newStudent = new Student
            {
                FullName = FullName,
                Group = Group,
                AverageScore = AverageScore
            };

            await _repository.SaveAsync(newStudent);
            Students.Add(newStudent);

            FullName = string.Empty;
            Group = string.Empty;
            AverageScore = 0;

            _appState.ClearDraft();
        }

        private async Task DeleteStudentAsync(Student? student)
        {
            if (student is null) return;
            await _repository.DeleteAsync(student);
            Students.Remove(student);
        }

        public void SaveDraftState()
        {
            if (string.IsNullOrWhiteSpace(FullName) && string.IsNullOrWhiteSpace(Group) && AverageScore == 0)
                return;
            _appState.SaveDraft(FullName, Group, AverageScore);
        }

        public void RestoreDraftState()
        {
            var draft = _appState.LoadDraft();
            if (draft is null) return;
            FullName = draft.Value.FullName;
            Group = draft.Value.Group;
            AverageScore = draft.Value.AverageScore;
        }

        private bool CanAddStudent() => !string.IsNullOrWhiteSpace(FullName);

        private async Task OpenDetailsAsync(Student? student)
        {
            if (student is null) return;

            var parameters = new Dictionary<string, object> { { "SelectedStudent", student } };
            for (int attempt = 0; attempt < 6; attempt++)
            {
                try { await Shell.Current.GoToAsync("studentdetail", parameters); return; }
                catch (InvalidOperationException) { await Task.Delay(250); }
            }
        }

        private async Task OpenDeepLinkedStudentAsync(Student target)
        {
            await Task.Delay(500);
            await OpenDetailsAsync(target);
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UpdatedStudent", out var value) && value is Student updated)
            {
                await _repository.SaveAsync(updated);
                int index = Students.IndexOf(updated);
                if (index >= 0)
                    Students[index] = updated;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}