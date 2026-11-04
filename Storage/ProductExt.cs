using Shop;

namespace Storage;

/// <summary>Методы расширения товара и склада.</summary>
public static class ProductExt
{
    /// <summary>Последний выданный идентификатор склада.</summary>
    private static int _lastStorageId;

    /// <summary>Возвращает следующий уникальный идентификатор склада.</summary>
    public static int NextStorageId() => ++_lastStorageId;

    /// <summary>Устанавливает текст кода товара по складу и позиции на нём.</summary>
    public static void SetTextForCode<T>(this T product, IStorage<T> storage, int index)
        where T : class, IProduct
    {
        if (product is null || storage is null)
        {
            return;
        }

        product.CodeMatrix.Text = $"{storage.Id}|[{index}]:{product.Id}:{product.Price}";
    }

    /// <summary>Устанавливает текст кода товара, зная склад, где он находится.</summary>
    public static void SetTextForCode<T>(this T product, IStorage<T> storage)
        where T : class, IProduct
    {
        if (product is null || storage is null)
        {
            return;
        }

        int index = storage.Find(product.Id);

        if (index < 0)
        {
            return;
        }

        product.SetTextForCode(storage, index);
    }

    /// <summary>Подписывает товар на изменение идентификатора склада и возвращает обработчик для отписки.</summary>
    public static Action<IStorage<T>> SubscribeToIdChange<T>(this T product, PackedStorage<T> storage)
        where T : class, INewProduct
    {
        storage.IdChanged+=product.SetTextForCode;
        return product.SetTextForCode;
    }

    /// <summary>Отписывает товар от изменения идентификатора склада.</summary>
    public static void UnSubscribeToIdChange<T>(
        this T product,
        PackedStorage<T> storage,
        Action<IStorage<T>> handler)
        where T : class, INewProduct
    {
        storage.IdChanged -= handler;
    }
}
