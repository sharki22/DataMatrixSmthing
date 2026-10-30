
namespace Shop;


/// <summary>Упаковка товара.</summary>
public sealed class Package : Product
{
    /// <summary>Наценка упаковки (110%).</summary>
    private const decimal Markup = 1.10m;

    /// <summary>Упаковываемый товар.</summary>
    private readonly IProduct _product;

    /// <summary>Создаёт упаковку для товара.</summary>
    public Package(IProduct product)
        : base(product.Id, product.Price * Markup, product.Name, new PackageCode())
    {
        _product = product;
    }

    /// <summary>Идентификатор упаковки совпадает с идентификатором товара.</summary>
    public override int Id
    {
        get => _product?.Id ?? base.Id;
        set
        {
            _product.Id = value;
            base.Id = value;
        }
    }


    /// <summary>Тип товара — упаковка.</summary>
    public override string Type => $"Упаковка ({_product.Type})";

    /// <summary>Информация о упакованном товаре.</summary>
    public override string Info => _product.Info;
}
