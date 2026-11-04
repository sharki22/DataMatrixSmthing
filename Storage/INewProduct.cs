using Shop;

namespace Storage;

/// <summary>Интерфейс товара с событием изменения данных.</summary>
public interface INewProduct : IProduct
{
    /// <summary>Событие изменения идентификатора или цены товара.</summary>
    event EventHandler<PackedArgs> ProductChanged;
}
