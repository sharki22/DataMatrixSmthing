
namespace Storage;

/// <summary>Запакованный обобщённый склад с подпиской на события товаров.</summary>
public sealed class PackedStorage<T> : Storage<T>, IDisposable
    where T : class, INewProduct
{
    /// <summary>Идентификатор склада.</summary>
    private int _id;

    /// <summary>Подписчики на изменение идентификатора склада.</summary>
    private Action<IStorage<T>>? _idChanged;

    /// <summary>Обработчики изменения данных товаров.</summary>
    private readonly Dictionary<T, EventHandler<PackedArgs>> _productChangedHandlers = [];

    /// <summary>Обработчики изменения идентификатора склада для товаров.</summary>
    private readonly Dictionary<T, Action<IStorage<T>>> _idChangedHandlers = [];

    /// <summary>Признак освобождения ресурсов.</summary>
    private bool _disposed;

    /// <summary>Создаёт склад заданного размера, при необходимости с указанным идентификатором.</summary>
    private PackedStorage((int size, int? id) tuple) : base(tuple.size)
    {
        _id = tuple.id ?? base.Id;
    }

    /// <summary>Создаёт склад заданного размера.</summary>
    public static implicit operator PackedStorage<T>(int size) => new((size, null));

    /// <summary>Создаёт склад заданного размера с указанным идентификатором.</summary>
    public static implicit operator PackedStorage<T>((int size, int id) tuple) => new((tuple.size, tuple.id));

    /// <summary>Событие смены идентификатора склада.</summary>
    public event Action<IStorage<T>>? IdChanged
    {
        add => _idChanged += value;
        remove => _idChanged -= value;
    }

    /// <summary>Идентификатор склада.</summary>
    public override int Id
    {
        get => _id;
        set
        {
            if (_id == value)
            {
                return;
            }

            _id = value;
            _idChanged?.Invoke(this);
        }
    }

    /// <summary>Товар в ячейке с указанным индексом.</summary>
    public override T? this[int index]
    {
        get
        {
            T? product = base[index];

            if (product is not null)
            {
                Unsubscribe(product);
            }

            return product;
        }
        set
        {
            if (index < 0 || index >= Size)
            {
                return;
            }

            if (_cells[index] is { } previous)
            {
                Unsubscribe(previous);
            }

            base[index] = value;

            if (value is not null)
            {
                Subscribe(value);
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

    /// <summary>Сортирует товары с использованием делегата сравнения.</summary>
    private void SortItems(Comparison<T> comparer)
    {
        for (int index = 1; index < _cells.Length; index++)
        {
            T? current = _cells[index];

            if (current is null)
            {
                continue;
            }

            int position = index - 1;

            while (position >= 0 &&
                   (_cells[position] is null || comparer(_cells[position]!, current) > 0))
            {
                _cells[position + 1] = _cells[position];
                position--;
            }

            _cells[position + 1] = current;
        }

        RefreshCodes();
    }

    /// <summary>Ищет позицию товара по идентификатору.</summary>
    public override int Find(int id) => FindIndex(product => product.Id == id);

    /// <summary>Ищет позицию товара по наименованию.</summary>
    public override int Find(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return -1;
        }

        return FindIndex(product =>
            string.Equals(product.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Ищет позицию товара с использованием делегата проверки.</summary>
    private int FindIndex(Predicate<T> match)
    {
        for (int index = 0; index < _cells.Length; index++)
        {
            if (_cells[index] is { } product && match(product))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Освобождает ресурсы и отписывается от всех событий.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var products = new T[_productChangedHandlers.Count];
        _productChangedHandlers.Keys.CopyTo(products, 0);

        foreach (T product in products)
        {
            Unsubscribe(product);
        }

        _productChangedHandlers.Clear();
        _idChangedHandlers.Clear();
        _idChanged = null;
        _disposed = true;
    }

    /// <summary>Подписывает товар на события склада.</summary>
    private void Subscribe(T product)
    {
        if (_idChangedHandlers.ContainsKey(product))
        {
            return;
        }

        Action<IStorage<T>> idHandler = product.SubscribeToIdChange(this);
        _idChangedHandlers[product] = idHandler;

        void productHandler(object? _1, PackedArgs _2) => RefreshProductCode(product);
        product.ProductChanged += productHandler;
        _productChangedHandlers[product] = productHandler;
    }

    /// <summary>Отписывает товар от событий склада.</summary>
    private void Unsubscribe(T product)
    {
        if (_idChangedHandlers.Remove(product, out Action<IStorage<T>>? idHandler))
        {
            product.UnSubscribeToIdChange(this, idHandler);
        }

        if (_productChangedHandlers.Remove(product, out EventHandler<PackedArgs>? productHandler))
        {
            product.ProductChanged -= productHandler;
        }
    }

    /// <summary>Обновляет код товара по его текущей позиции на складе.</summary>
    private void RefreshProductCode(T product)
    {
        int index = IndexOf(product);

        if (index >= 0)
        {
            product.SetTextForCode(this, index);
        }
    }

    /// <summary>Возвращает текущую позицию товара на складе.</summary>
    private int IndexOf(T product)
    {
        for (int index = 0; index < _cells.Length; index++)
        {
            if (ReferenceEquals(_cells[index], product))
            {
                return index;
            }
        }

        return -1;
    }
}
