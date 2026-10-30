namespace Shop;

using DataMatrix;

/// <summary>Интерфейс товара.</summary>
public interface IProduct
{
    /// <summary>Идентификатор товара.</summary>
    int Id { get; set; }

    /// <summary>Цена товара.</summary>
    decimal Price { get; set; }

    /// <summary>Наименование товара.</summary>
    string Name { get; }

    /// <summary>Код товара.</summary>
    IDataMatrix CodeMatrix { get; }

    /// <summary>Тип товара.</summary>
    string Type { get; }

    /// <summary>Дополнительная информация по товару.</summary>
    string Info { get; }
}
