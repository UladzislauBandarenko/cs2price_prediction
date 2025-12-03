namespace cs2price_prediction.DTOs.Meta
{
    /// <summary>
    /// Универсальный вариант "паттерн / фаза" для выбора пользователем.
    /// Для ch_*/fade_*: Id = pattern, Name = "661" и т.п.
    /// Для doppler:    Id = phaseId, Name = "Ruby", "Sapphire" и т.п.
    /// Для float_gun и прочих, где паттерн не нужен — список будет пустой.
    /// </summary>
    public record PatternOptionDto(
        int Id,
        string Name
    );
}
