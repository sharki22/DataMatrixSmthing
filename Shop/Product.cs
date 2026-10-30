namespace Shop;

using DataMatrix;

/// <summary>Товар.</summary>
public abstract class Product : IProduct
{
    /// <summary>Код товара.</summary>
    public IDataMatrix CodeMatrix {get;}

    /// <summary>Имя товара.</summary>
    public string Name { get; }

    /// <summary>Идентификатор товара.</summary>
    private int _id;
    /// <summary>Цена товара.</summary>
     private decimal _price;

    /// <summary>Создаёт товар.</summary>
    protected Product(int id, decimal price, string name, IDataMatrix code)
    {
        _id = id;
        _price = price;
        Name = name;
        CodeMatrix = code;

        UpdateCode();
    }

    /// <summary>Идентификатор товара.</summary>
    public virtual int Id
    {
        get => _id;
        set
        {
            _id = value;
            UpdateCode();
        }
    }

    /// <summary>Цена товара.</summary>
    public virtual decimal Price
    {
        get => _price;
        set
        {
            _price = value < 0 ? 0 : value;
            UpdateCode();
        }
    }



    /// <summary>Тип товара.</summary>
    public abstract string Type { get; }

    /// <summary>Дополнительная информация о товаре.</summary>
    public abstract string Info { get; }


    /// <summary>Строка с типом, именем, данными и кодом товара.</summary>
    public sealed override string ToString() => $"{Type}:{Name}\n{Info}\n{CodeMatrix}";

    /// <summary>Обновляет текст кода по Id и цене.</summary>
    private void UpdateCode() => CodeMatrix.Text = $"{_id}:{_price}";
}
