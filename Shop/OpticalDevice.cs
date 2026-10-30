namespace Shop;

/// <summary>Оптический прибор.</summary>
public sealed class OpticalDevice : Product
{
    /// <summary>Модель прибора.</summary>
    public string Model { get; }

    /// <summary>Оптический тип прибора.</summary>
    public OpticalType DeviceType { get; }

    /// <summary>Материал корпуса.</summary>
    public string Material { get; }

    /// <summary>Поле зрения в градусах.</summary>
    public double FieldOfView { get; }

    /// <summary>Увеличение прибора.</summary>
    public double Magnification { get; }

    /// <summary>Создаёт оптический прибор.</summary>
    public OpticalDevice(
        int id,
        decimal price,
        string name,
        string model,
        OpticalType deviceType,
        string material,
        double fieldOfView,
        double magnification)
        : base(id, price, name, new DataMatrix.DataMatrix())
    {
        Model = model;
        DeviceType = deviceType;
        Material = material;
        FieldOfView = fieldOfView;
        Magnification = magnification;
    }

    /// <summary>Тип товара — «Оптический прибор».</summary>
    public override string Type => "Оптический прибор";

    /// <summary>Описание прибора.</summary>
    public override string Info =>
        $"Модель: {Model}\nТип: {DeviceType}\nМатериал: {Material}\nПоле зрения: {FieldOfView}°\nУвеличение: {Magnification}x";
}
