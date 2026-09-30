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

        private int _nextStudentId = 1;

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
        public ICommand LoadCommand { get; }

        public StudentViewModel()
        {
            AddStudentCommand = new Command(AddStudent, CanAddStudent);
            OpenDetailsCommand = new Command<Student>(async (student) => await OpenDetailsAsync(student));
            LoadCommand = new Command(Load);
        }

        private void AddStudent()
        {
            Students.Add(new Student
            {
                Id = _nextStudentId++,
                FullName = FullName,
                Group = Group,
                AverageScore = AverageScore
            });

            FullName = string.Empty;
            Group = string.Empty;
            AverageScore = 0;

            _appState.ClearDraft();
        }

        private void Load()
        {
            if (_appState.PendingDeepLinkStudentId is int pendingId)
            {
                _appState.PendingDeepLinkStudentId = null;
                var target = Students.FirstOrDefault(s => s.Id == pendingId);
                if (target is not null)
                {
                    _ = OpenDeepLinkedStudentAsync(target);
                }
            }
        }

        private async Task OpenDeepLinkedStudentAsync(Student target)
        {
            await Task.Delay(500);
            await OpenDetailsAsync(target);
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

            var parameters = new Dictionary<string, object>
            {
                { "SelectedStudent", student }
            };

            for (int attempt = 0; attempt < 6; attempt++)
            {
                try
                {
                    await Shell.Current.GoToAsync("studentdetail", parameters);
                    return;
                }
                catch (InvalidOperationException)
                {
                    await Task.Delay(250);
                }
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UpdatedStudent", out var value) && value is Student updated)
            {
                int index = Students.IndexOf(updated);
                if (index >= 0)
                {
                    Students[index] = updated;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}