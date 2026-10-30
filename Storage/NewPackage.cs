using DataMatrix;
using Shop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Storage;

public class NewPackage : NewProduct,IDisposable
{

    /// <summary>Наценка упаковки (110%).</summary>
    private const decimal Markup = 1.10m;

    /// <summary>Упаковываемый товар.</summary>
    private readonly INewProduct _product;
    public event EventHandler<PackedArgs>? ProductChanged;
    public NewPackage(INewProduct product) : base(product.Id,product.Price, product.Name,product.CodeMatrix)
    {
        _product=product;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
