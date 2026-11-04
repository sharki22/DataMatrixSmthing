using Shop;

namespace Storage;

/// <summary>Запакованный оптический прибор с событием изменения данных.</summary>
public sealed class NewOpticalDevice : OpticalDevice, INewProduct
{
    /// <summary>Создаёт запакованный оптический прибор.</summary>
    public NewOpticalDevice(
        int id,
        decimal price,
        string name,
        string model,
        OpticalType deviceType,
        string material,
        double fieldOfView,
        double magnification)
        : base(id, price, name, model, deviceType, material, fieldOfView, magnification)
    {
    }

    /// <summary>Событие изменения идентификатора или цены прибора.</summary>
    public event EventHandler<PackedArgs>? ProductChanged;

    /// <summary>Идентификатор прибора.</summary>
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

    /// <summary>Цена прибора.</summary>
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

    /// <summary>Информирует подписчиков об изменении данных прибора.</summary>
    private void RaiseProductChanged() =>
        ProductChanged?.Invoke(this, new PackedArgs(Id, Price));
}
