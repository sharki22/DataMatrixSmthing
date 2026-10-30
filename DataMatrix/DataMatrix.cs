namespace DataMatrix;

/// <summary>Символ Data Matrix.</summary>
public sealed class DataMatrix : IDataMatrix
{
    /// <summary>Исходный текст.</summary>
    private string _text = string.Empty;

    /// <summary>Высота символа в модулях.</summary>
    private int? _rows;

    /// <summary>Ширина символа в модулях.</summary>
    private int? _columns;

    /// <summary>Инвертировать отрисовку символа.</summary>
    private bool _invert;

    /// <summary>Размер символа в модулях.</summary>
    public string Version { get; private set; } = string.Empty;

    /// <summary>Уровень коррекции ошибок.</summary>
    public int CorrectionLevel { get; private set; }

    /// <summary>Маска символа (всегда 0).</summary>
    public int Mask => 0;

    /// <summary>Исходный текст.</summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            Generate();
        }
    }

    /// <summary>Символ Data Matrix.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Высота символа в модулях.</summary>
    public int? Rows
    {
        get => _rows;
        set
        {
            _rows = value;
            Generate();
        }
    }

    /// <summary>Ширина символа в модулях.</summary>
    public int? Columns
    {
        get => _columns;
        set
        {
            _columns = value;
            Generate();
        }
    }

    /// <summary>Инвертировать отрисовку символа.</summary>
    public bool Invert
    {
        get => _invert;
        set
        {
            _invert = value;
            Generate();
        }
    }

    /// <summary>Вывод символа в выбранном формате.</summary>
    public override string ToString() => IDataMatrix.OutputMode switch
    {
        CodeType.Text => Text,
        CodeType.Code => Code,
        CodeType.Full => string.IsNullOrEmpty(Code) ? Text : $"{Code}\n{DataMatrixBuilder.CenterText(Code, Text)}",
        _ => Code,
    };

    /// <summary>Перестраивает символ по тексту.</summary>
    private void Generate()
    {
        Code = Text.ToDataMatrix(_rows, _columns, _invert);

        var (height, width) = Text.GetSymbolSize(_rows, _columns);

        Version = $"{height}×{width}";

        CorrectionLevel = Text.GetCorrectionLevel(_rows, _columns);
    }
}
