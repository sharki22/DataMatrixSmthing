using System.Text;
using Shop;

namespace Storage;

/// <summary>Обобщённый склад с фиксированным количеством ячеек.</summary>
public class Storage<T> : IStorage<T>
    where T : class, IProduct
{
    /// <summary>Ячейки склада. Пустая ячейка содержит null.</summary>
    private readonly T?[] _cells;

    /// <summary>Числовой идентификатор склада.</summary>
    private int _id;

    /// <summary>Создаёт склад с фиксированным количеством ячеек.</summary>
    protected Storage(int size)
    {
        _cells = new T?[size < 0 ? 0 : size];
        _id = ProductExt.NextStorageId();
    }

    /// <summary>Создаёт склад из целого числа ячеек.</summary>
    public static implicit operator Storage<T>(int size) => new(size);

    /// <summary>Уникальный числовой идентификатор склада.</summary>
    public virtual int Id
    {
        get => _id;
        set
        {
            if (_id == value)
            {
                return;
            }

            _id = value;
            RefreshCodes();
        }
    }

    /// <summary>Информация о складе по его идентификатору.</summary>
    public string GetInfo => $"Склад {Id}: занято {Count} из {Size} ячеек";

    /// <summary>Количество ячеек на складе.</summary>
    public int Size => _cells.Length;

    /// <summary>Количество занятых ячеек.</summary>
    public int Count
    {
        get
        {
            int count = 0;

            foreach (T? product in _cells)
            {
                if (product is not null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>индексатор.</summary>
    public virtual T? this[int index]
    {
        get
        {
            if (index < 0 || index >= _cells.Length)
            {
                return null;
            }

            T? product = _cells[index];

            if (product is null)
            {
                return null;
            }

            _cells[index] = null;
            return product;
        }
        set
        {
            if (index < 0 || index >= _cells.Length)
            {
                return;
            }

            _cells[index] = value;
            value?.SetTextForCode(this, index);
        }
    }

    /// <summary>Добавляет товар в первую свободную ячейку.</summary>
    public bool Push(T product)
    {
        if (product is null)
        {
            return false;
        }

        for (int index = 0; index < _cells.Length; index++)
        {
            if (_cells[index] is null)
            {
                this[index] = product;
                return true;
            }
        }

        return false;
    }

    /// <summary>Добавляет товар в конкретную свободную ячейку.</summary>
    public bool Insert(int index, T product)
    {
        if (product is null || index < 0 || index >= _cells.Length || _cells[index] is not null)
        {
            return false;
        }

        this[index] = product;
        return true;
    }

    /// <summary>Снимает товар из ячейки и возвращает его.</summary>
    public T? Remove(int index) => this[index];

    /// <summary>Меняет местами товары в двух ячейках.</summary>
    public bool Swap(int first, int second)
    {
        if (first < 0 || first >= _cells.Length || second < 0 || second >= _cells.Length)
        {
            return false;
        }

        if (first == second)
        {
            return true;
        }

        (this[second], this[first]) = (this[first], this[second]);
        return true;
    }

    /// <summary>Заменяет содержимое ячейки новым товаром.</summary>
    public bool Replace(int index, T product)
    {
        if (product is null || index < 0 || index >= _cells.Length)
        {
            return false;
        }

        this[index] = product;
        return true;
    }

    /// <summary>Ищет позицию товара по его идентификатору.</summary>
    public virtual int Find(int id)
    {
        for (int index = 0; index < _cells.Length; index++)
        {
            if (_cells[index] is { } product && product.Id == id)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Ищет позицию товара по его наименованию.</summary>
    public virtual int Find(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return -1;
        }

        for (int index = 0; index < _cells.Length; index++)
        {
            if (_cells[index] is { } product &&
                string.Equals(product.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Сортирует товары на складе по идентификатору.</summary>
    public virtual void OrderById() =>
        SortItems(static (left, right) => left.Id.CompareTo(right.Id));

    /// <summary>Сортирует товары на складе по наименованию.</summary>
    public virtual void OrderByName() =>
        SortItems(static (left, right) =>
            string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Обновляет текст кода всех товаров на складе.</summary>
    public void RefreshCodes()
    {
        for (int index = 0; index < _cells.Length; index++)
        {
            _cells[index]?.SetTextForCode(this, index);
        }
    }

    /// <summary>Информация по всем элементам склада, включая пустые ячейки.</summary>
    public sealed override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine(GetInfo);

        for (int index = 0; index < _cells.Length; index++)
        {
            T? product = _cells[index];

            builder.Append('[')
                .Append(index)
                .Append("]: ")
                .AppendLine(product is null ? "<пусто>" : product.ToString());
        }

        return builder.ToString();
    }

    /// <summary>Сортировка товаров вставками, пустые ячейки перемещаются в конец.</summary>
    private void SortItems(Comparison<T> comparison)
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
                   (_cells[position] is null || comparison(_cells[position]!, current) > 0))
            {
                _cells[position + 1] = _cells[position];
                position--;
            }

            _cells[position + 1] = current;
        }

        RefreshCodes();
    }
}
