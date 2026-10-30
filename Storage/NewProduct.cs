using DataMatrix;
using Shop;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Storage;

public class NewProduct : INewProduct
{


    protected NewProduct(int id, decimal price, string name, IDataMatrix code)
    {
        Id= id;
        Price= price;
        Name = name;
        CodeMatrix = code;

        UpdateCode();
    }
    public int Id { get; 
        set
        {
            if (Id == value) return;
            Id = value;
            RaiseProductChanged();
        }

    }
    public decimal Price { get ; set
        {
            if(Price == value) return;
            Price=value; RaiseProductChanged();

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

