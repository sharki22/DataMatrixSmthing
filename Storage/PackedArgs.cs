namespace Storage;

/// <summary>Аргументы события изменения данных товара.</summary>
public sealed class PackedArgs(int newId, decimal newPrice) : EventArgs
{
    /// <summary>Новый идентификатор товара.</summary>
    public int NewId { get; } = newId;

    /// <summary>Новая цена товара.</summary>
    public decimal NewPrice { get; } = newPrice;
}
