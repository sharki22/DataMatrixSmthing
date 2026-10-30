namespace DataMatrix;

/// <summary>Формат вывода символа Data Matrix.</summary>
public enum CodeType
{
    /// <summary>Только исходный текст.</summary>
    Text,

    /// <summary>Только отрисованный символ.</summary>
    Code,

    /// <summary>Символ и текст вместе.</summary>
    Full
}
