using Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public class PackedStorage<T> : Storage<T>
        where T : class, IProduct
    {
        
        ///<inheritdoc/>
        private readonly T[] _cells;
        ///<inheritdoc/>
        private PackedStorage((int size, int? Id) tup) : base(tup.size)
        {
            if (tup.Id is not null)
            {
                Id = tup.Id.Value;
            }
            Id = ProductExt.NextStorageId();

        }
        /// <summary> словарь для хранения подписчиков на делегат</summary>
        
        private readonly Dictionary<T, Action<IStorage<T>>> _subscriptions = new();


        /// <summary Делегат для уведомления о смене идентификатора </summary>
        public Action<IStorage<T>>? IdChanged;

        /// <summary>
        /// Идентификатор
        /// </summary>
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


        /// <summary>Сортирует товары на складе по идентификатору.</summary>
        public override void OrderById() =>
            SortItems(static (left, right) => left.Id.CompareTo(right.Id));

        /// <summary>Сортирует товары на складе по наименованию.</summary>
        public override void OrderByName() =>
            SortItems(static (left, right) =>
                string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));


        /// <summary>Сортировка с использованием делегата</summary>

        private void SortItems(Comparison<T> comparer)
        {
            for (int index = 1; index <= _cells.Length; index++)
            {
                var current = _cells[index];
                if (current is null) continue;
                int pos = index - 1;

                while (pos >= 0 &&
                   (_cells[pos] is null || comparer(_cells[pos]!, current) > 0))
                {
                    _cells[pos + 1] = _cells[pos];
                    pos--;
                }

                _cells[pos + 1] = current;
            }
            RefreshCodes();
        }
        /// <summary>
        /// поиск по ID
        /// </summary>
        
        public override int Find(int id)=>FindIndex(prod=>prod.Id==id);
        /// <summary>
        /// Поиск по имени
        /// </summary>
        public override int Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            return FindIndex(prod=>string.Equals(prod.Name, name, StringComparison.OrdinalIgnoreCase));
        
        }


        private int FindIndex(Predicate<T> match)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] is { } pruct && match(pruct))
                {
                    return i;
                }
            }

               return -1;
        }







    }
}


