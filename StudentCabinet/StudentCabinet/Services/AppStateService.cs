using Microsoft.Maui.Storage;

namespace StudentCabinet.Services
{
    /// <summary>
    /// Лабораторна №3, п.4: конкретна реалізація IAppStateService.
    /// Реєструється в MauiProgram.cs як singleton.
    ///
    /// Чернетка форми зберігається через Preferences (Microsoft.Maui.Storage) —
    /// це проста персистентність "ключ-значення", яка переживає навіть повне
    /// закриття застосунку, а не лише згортання у фоновий режим.
    /// PendingDeepLinkStudentId навмисно НЕ зберігається у Preferences — це
    /// одноразова "команда" на поточний запуск застосунку, тому досить звичайного
    /// поля в пам'яті синглтона.
    /// </summary>
    public class AppStateService : IAppStateService
    {
        private const string FullNameKey = "draft_full_name";
        private const string GroupKey = "draft_group";
        private const string AvgKey = "draft_average_score";
        private const string HasDraftKey = "draft_has_value";

        /// <summary>
        /// Лабораторна №3, п.6: id студента, якого потрібно відкрити за deep link.
        /// Живе тільки в пам'яті — його читає і одразу очищає StudentViewModel.LoadCommand.
        /// </summary>
        public int? PendingDeepLinkStudentId { get; set; }

        public void SaveDraft(string fullName, string group, double averageScore)
        {
            Preferences.Set(FullNameKey, fullName ?? string.Empty);
            Preferences.Set(GroupKey, group ?? string.Empty);
            Preferences.Set(AvgKey, averageScore);
            Preferences.Set(HasDraftKey, true);
        }

        public (string FullName, string Group, double AverageScore)? LoadDraft()
        {
            if (!Preferences.Get(HasDraftKey, false))
            {
                return null;
            }

            return (
                Preferences.Get(FullNameKey, string.Empty),
                Preferences.Get(GroupKey, string.Empty),
                Preferences.Get(AvgKey, 0.0)
            );
        }

        public void ClearDraft()
        {
            Preferences.Remove(FullNameKey);
            Preferences.Remove(GroupKey);
            Preferences.Remove(AvgKey);
            Preferences.Remove(HasDraftKey);
        }
    }
}