using System;
using System.Collections.Generic;
using System.Text;

namespace Storage;

public class PackedArgs : EventArgs
{
    public int NewId { get; set; }
    public decimal NewPrice { get; set; }


    public PackedArgs(int newId, decimal newPrice)
    {
        NewId = newId;
        newPrice = NewPrice;
    }

}
