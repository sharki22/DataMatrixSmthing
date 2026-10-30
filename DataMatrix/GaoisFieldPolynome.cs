namespace DataMatrix;

/// <summary>Многочлен над полем Галлуа.</summary>
internal record GaoisFieldPolynome(int[] Coefficients)
{
    /// <summary>Коэффициенты многочлена от старшего к младшему.</summary>
    public int[] Coefficients { get; init; } = Coefficients.Length > 1
        ? [.. Coefficients.SkipWhile(x => x == 0)]
        : Coefficients;

    /// <summary>Преобразует число в многочлен нулевой степени.</summary>
    public static implicit operator GaoisFieldPolynome(int zero) => new([zero]);
}
