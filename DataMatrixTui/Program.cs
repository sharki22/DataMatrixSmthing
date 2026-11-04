using Shop;
using Storage;

namespace DataMatrix.Tui;

/// <summary>Терминал ассортимента товаров.</summary>
internal static class Program
{
    /// <summary>Запускает терминал.</summary>
    private static void Main()
    {

    }
    /// <summary>Переключает режим вывода Data Matrix по кругу.</summary>
    private static void ChangeOutputMode()
    {
        IDataMatrix.OutputMode = IDataMatrix.OutputMode switch
        {
            CodeType.Full => CodeType.Text,
            CodeType.Text => CodeType.Code,
            _ => CodeType.Full,
        };

        Console.WriteLine($"Текущий режим вывода: {IDataMatrix.OutputMode}.");
    }

    private static void RunLab3()
    {
        var binoculars = new OpticalDevice(
            id: 1,
            price: 1000,
            name: "Бинокль",
            model: "БПЦ 10x40",
            deviceType: OpticalType.Lens,
            material: "Металл",
            fieldOfView: 6.5,
            magnification: 10.0);

        var telescope = new OpticalDevice(
            id: 2,
            price: 1500,
            name: "Телескоп",
            model: "Sky-Watcher BK P15012",
            deviceType: OpticalType.Mirror,
            material: "Сталь",
            fieldOfView: 1.2,
            magnification: 48.0);

        var telescopePackage = new Package(telescope);

        IStorage<IProduct> storage1 = (Storage<IProduct>)2;
        IStorage<Package> storage2 = (Storage<Package>)1;


        storage1[0] = binoculars;
        storage1.Push(telescopePackage);
        storage1.OrderByName();
        Console.WriteLine(storage1);

        storage1.Id = 100;
        Console.WriteLine(storage1);

        var package = storage1[storage1.Find(telescopePackage.Id)];
        storage2[0] = package as Package;
        Console.WriteLine(storage1);
        Console.WriteLine();
        Console.WriteLine(storage2);
    }

    /// <summary>Пример четвёртой лабораторной работы.</summary>
    private static void RunLab4()
    {
        var binoculars = new NewOpticalDevice(
            id: 1,
            price: 1000,
            name: "Бинокль",
            model: "БПЦ 10x40",
            deviceType: OpticalType.Lens,
            material: "Металл",
            fieldOfView: 6.5,
            magnification: 10.0);

        var telescope = new NewOpticalDevice(
            id: 2,
            price: 1500,
            name: "Телескоп",
            model: "Sky-Watcher BK P15012",
            deviceType: OpticalType.Mirror,
            material: "Сталь",
            fieldOfView: 1.2,
            magnification: 48.0);

        using var telescopePackage = new NewPackage(telescope);
        using var assortment = (PackedStorage<INewProduct>)2;

        assortment[0] = binoculars;
        assortment.Push(telescopePackage);

        Console.WriteLine("До изменения");
        Console.WriteLine(assortment);

        binoculars.Id = 300;
        binoculars.Price = 300;
        telescope.Id = 400;
        telescope.Price = 400;

        Console.WriteLine("После изменения Id и Price");
        Console.WriteLine(assortment);

        assortment.Id = 100;

        Console.WriteLine("После изменения Id склада");
        Console.WriteLine(assortment);
    }
}
