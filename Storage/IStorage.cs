using Shop;

namespace Storage;

/// <summary>Обобщённый интерфейс склада.</summary>
public interface IStorage<T>
    where T : class, IProduct
{
    /// <summary>Уникальный числовой идентификатор склада.</summary>
    int Id { get; set; }

    /// <summary>Информация о складе по его идентификатору.</summary>
    string GetInfo { get; }

    /// <summary>Количество ячеек на складе.</summary>
    int Size { get; }

    /// <summary>Товар в ячейке с указанным индексом.</summary>
    T? this[int index] { get; set; }

    /// <summary>Добавляет товар в первую свободную ячейку.</summary>
    bool Push(T product);

    /// <summary>Добавляет товар в конкретную свободную ячейку.</summary>
    bool Insert(int index, T product);

    /// <summary>Снимает товар из ячейки и возвращает его.</summary>
    T? Remove(int index);

    /// <summary>Меняет местами товары в двух ячейках.</summary>
    bool Swap(int first, int second);

    /// <summary>Заменяет содержимое ячейки новым товаром.</summary>
    bool Replace(int index, T product);

    /// <summary>Ищет позицию товара по его идентификатору.</summary>
    int Find(int id);

    /// <summary>Ищет позицию товара по его наименованию.</summary>
    int Find(string name);

    /// <summary>Сортирует товары на складе по идентификатору.</summary>
    void OrderById();

    /// <summary>Сортирует товары на складе по наименованию.</summary>
    void OrderByName();
}
