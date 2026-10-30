using Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public class PackedStorage<T> : Storage<T>
        where T : class, IProduct
    {


        private PackedStorage((int size, int? Id) tup) : base(tup.size)
        {
            if (tup.Id is null)
            {
                Id = ProductExt.NextStorageId();

                Id = tup.Id.Value;

            }
        }

        private readonly Dictionary<T, Action<IStorage<T>>> _subscriptions = new();

        public Action<IStorage<T>>? IdChanged;
        public override int Id
        {
            get => Id;
            set
            {
                if (Id == value) return;


                Id = value;
                IdChanged?.Invoke(this);


            }
        }

        public override T? this[int index]
        {

            get
            {

                T? product = base[index];
                if (product is not null && _subscriptions.TryGetValue(product, out var handler))
                {
                    product.UnSubscribeToIdChange(this, handler);
                    _subscriptions.Remove(product);
                }
                return product;
            }
            set
            {
                if (index < 0 || index >= Size)
                {
                    T? old = base[index];
                    if (old is not null && _subscriptions.TryGetValue(old, out var handler))
                    {
                        old.UnSubscribeToIdChange(this, handler);
                        _subscriptions.Remove(old);
                    }
                    base[index] = value;
                    if (value is not null)
                    {
                        var handl = value.SubscribeToIdChange(this);
                        _subscriptions[value] = handl;
                    }

                }
            }

        }
    }
}
