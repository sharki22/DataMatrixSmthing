
namespace Shop;


/// <summary>Упаковка товара.</summary>
/// <remarks>Создаёт упаковку для товара.</remarks>
public class Package(IProduct product) : Product(product.Id, product.Price * Markup, product.Name, new PackageCode())
{
    /// <summary>Наценка упаковки (110%).</summary>
    private const decimal Markup = 1.10m;

    /// <summary>Упаковываемый товар.</summary>
    private readonly IProduct _product = product;

    /// <summary>Идентификатор упаковки .</summary>
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
