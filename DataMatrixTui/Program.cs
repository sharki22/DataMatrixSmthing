using Shop;
using Storage;

namespace DataMatrix.Tui;

/// <summary>Терминал ассортимента товаров.</summary>
internal static class Program
{
    /// <summary>Запускает терминал.</summary>
    private static void Main()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Терминал ассортимента товаров");
            Console.WriteLine("1. Лабораторная работа №2");
            Console.WriteLine("2. Лабораторная работа №3");
            Console.WriteLine("3. Сменить режим вывода Data Matrix");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice?.Trim())
            {
                case "1":
                    RunLab2();
                    break;

                case "2":
                    RunLab3();
                    break;

                case "3":
                    ChangeOutputMode();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }
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

    /// <summary>Пример второй лабораторной работы.</summary>
    private static void RunLab2()
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

        IProduct[] products = [binoculars, telescope];

        Console.WriteLine("До изменения");

        foreach (IProduct product in products)
            Console.WriteLine(product);

        foreach (IProduct product in products)
        {
            product.Id++;
            product.Price += 1;
        }

        Console.WriteLine("После изменения Id и Price");

        foreach (IProduct product in products)
            Console.WriteLine(product);

        IProduct[] packageproducts = [new Package(binoculars), new Package(telescope)];

        foreach (IProduct product in packageproducts)
            Console.WriteLine(product);
    }
    /// <summary>Пример третьей лабораторной работы.</summary>
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

}
