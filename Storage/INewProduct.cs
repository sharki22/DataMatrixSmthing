using Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage;


public interface INewProduct : IProduct
{
    event EventHandler<PackedArgs> ProductChanged;
}

