using DataMatrix;
using Shop;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Storage;

public class NewProduct : INewProduct
{

    private int _id;
    private decimal _price;


    protected NewProduct(int id, decimal price, string name, IDataMatrix code)
    {
        Id= id;
        Price= price;
        Name = name;
        CodeMatrix = code;

        UpdateCode();
    }
    public int Id { get=>_id; 
        set
        {
            if (_id == value) return;
            _id = value;
            RaiseProductChanged();
        }

    }
    public decimal Price { get=>_price ; set
        {
            if(_price == value) return;
            _price = value; RaiseProductChanged();

        }
    }

    public string Name { get; }

    public IDataMatrix CodeMatrix { get; set; }

    public string Type { get;set;  }

    public string Info { get; set; }

    public event EventHandler<PackedArgs> ProductChanged;


    private void RaiseProductChanged() => ProductChanged?.Invoke(this, new PackedArgs(Id, Price));
    private void UpdateCode() => CodeMatrix.Text = $"{Id}:{Price}";
}

