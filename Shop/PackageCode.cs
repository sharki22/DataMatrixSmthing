namespace Shop;

using DataMatrix;

/// <summary>Код упаковки.</summary>
public sealed class PackageCode : IDataMatrix
{
    /// <summary>Собственный код Data Matrix.</summary>
    private readonly DataMatrix _ownCode = new();

    /// <summary>Текст кода.</summary>
    public string Text
    {
        get => _ownCode.Text;
        set => _ownCode.Text = value;
    }

    /// <summary>Символ Data Matrix.</summary>
    public string Code => _ownCode.Code;

    /// <summary>Текст кода со звёздочками: «* Текст *».</summary>
    public override string ToString() => IDataMatrix.OutputMode switch
    {
        CodeType.Text => $"* {_ownCode.Text} *",
        CodeType.Code => _ownCode.Code,
        CodeType.Full => $"{_ownCode.Code}\n{DataMatrixBuilder.CenterText(_ownCode.Code, $"* {_ownCode.Text} *")}",
        _ => _ownCode.ToString(),
    };
}
