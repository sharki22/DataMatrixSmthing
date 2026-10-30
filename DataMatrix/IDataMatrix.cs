namespace DataMatrix;

/// <summary>Код Data Matrix.</summary>
public interface IDataMatrix
{
    /// <summary>Хранит выбранный формат вывода.</summary>
    private static CodeType _outputMode = CodeType.Full;

    /// <summary>Формат вывода для всех кодов.</summary>
    static CodeType OutputMode
    {
        get => _outputMode;
        set => _outputMode = value;
    }
    /// <summary>Исходный текст.</summary>
    string Text { get; set; }

    /// <summary>Символ Data Matrix.</summary>
    string Code { get; }
}
