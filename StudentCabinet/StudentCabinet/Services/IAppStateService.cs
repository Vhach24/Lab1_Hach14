namespace StudentCabinet.Services
{
    /// <summary>
    /// Лабораторна №3, п.4: сервіс стану застосунку. Реєструється в MauiProgram.cs
    /// як singleton (один спільний екземпляр на весь застосунок), тому будь-яка
    /// частина застосунку, яка отримує цей сервіс, працює з тими самими даними.
    ///
    /// Сервіс навмисно нічого не знає про ViewModel чи View — лише зберігає
    /// прості дані (draft-форму та id для deep link), а сама Preferences-персистентність
    /// дозволяє пережити навіть повне закриття застосунку, не лише згортання.
    /// </summary>
    public interface IAppStateService
    {
        /// <summary>
        /// Лабораторна №3, п.2: зберегти незбережені дані форми додавання студента
        /// (викликається при згортанні застосунку — Window.Stopped).
        /// </summary>
        void SaveDraft(string fullName, string group, double averageScore);

        /// <summary>
        /// Повернути раніше збережений чернетковий стан форми, якщо він є
        /// (викликається при поверненні застосунку на передній план — Window.Resumed).
        /// </summary>
        (string FullName, string Group, double AverageScore)? LoadDraft();

        /// <summary>
        /// Прибрати збережену чернетку (наприклад, після успішного додавання студента).
        /// </summary>
        void ClearDraft();

        /// <summary>
        /// Лабораторна №3, п.6: ідентифікатор студента, якого потрібно відкрити
        /// за deep link-посиланням, щойно список студентів буде готовий.
        /// </summary>
        int? PendingDeepLinkStudentId { get; set; }
    }
}