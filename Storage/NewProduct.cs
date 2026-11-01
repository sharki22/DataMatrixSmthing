using Shop;

namespace Storage;

/// <summary>Запакованный товар с событием изменения данных.</summary>
/// <remarks>Создаёт товар с автоматически сформированным кодом Data Matrix.</remarks>
public sealed class NewProduct(int id, decimal price, string name, string type, string info) : Product(id, price, name, new DataMatrix.DataMatrix()), INewProduct
{
    /// <summary>Тип товара.</summary>
    private readonly string _type = type;

    /// <summary>Дополнительная информация о товаре.</summary>
    private readonly string _info = info;

    /// <summary>Событие изменения идентификатора или цены товара.</summary>
    public event EventHandler<PackedArgs>? ProductChanged;

    /// <summary>Идентификатор товара.</summary>
    public override int Id
    {
        get => base.Id;
        set
        {
            if (base.Id == value)
            {
                return;
            }

            base.Id = value;
            RaiseProductChanged();
        }
    }
    /// <summary>Цена товара.</summary>
    public override decimal Price
    {
        get => base.Price;
        set
        {
            decimal price = value < 0 ? 0 : value;

            if (base.Price == price)
            {
                return;
            }
  
            base.Price = price;
            RaiseProductChanged();
        }
    }

    /// <summary>Тип товара.</summary>
    public override string Type => _type;

    /// <summary>Дополнительная информация о товаре.</summary>
    public override string Info => _info;

    /// <summary>Информирует подписчиков об изменении данных товара.</summary>
    private void RaiseProductChanged() {
        ProductChanged?.Invoke(this, new PackedArgs(Id, Price));
        

    }
}
