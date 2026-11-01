using Shop;

namespace Storage;

/// <summary>Запакованная упаковка, обновляющая данные при изменении товара.</summary>
public sealed class NewPackage : Package, INewProduct, IDisposable
{
    /// <summary>Наценка упаковки (110%).</summary>
    private const decimal Markup = 1.10m;

    /// <summary>Упаковываемый товар.</summary>
    private readonly INewProduct _product;

    /// <summary>Признак освобождения ресурсов.</summary>
    private bool _disposed;

    /// <summary>Создаёт упаковку для товара.</summary>
    public NewPackage(INewProduct product)
        : base(product)
    {
        _product = product;
        _product.ProductChanged += OnProductChanged;
    }

    /// <summary>Событие изменения идентификатора или цены упаковки.</summary>
    public event EventHandler<PackedArgs>? ProductChanged;

    /// <summary>Освобождает ресурсы и отписывается от события товара.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
    
        _product.ProductChanged -= OnProductChanged;
        _disposed = true;

    }
   
    public override decimal Price{
      get=>base.Price;

      set{
      if (base.Price==value)return;
      base.Price=value;

      ProductChanged?.Invoke(this, new PackedArgs(Id,Price));
      }
    }


    public override int Id{
      get=>base.Id;

      set{
      if (base.Id==value)return;
      base.Id=value;

      ProductChanged?.Invoke(this, new PackedArgs(Id,Price));
      }
    }


    /// <summary>Обновляет данные упаковки по изменённому товару.</summary>
    private void OnProductChanged(object? sender, PackedArgs e)
    {
        Id = e.NewId;

        decimal price = e.NewPrice * Markup;

        if (price != Price)
        {
            Price = price;
        }
        ProductChanged?.Invoke(this, new PackedArgs(Id, Price));
    }
}
