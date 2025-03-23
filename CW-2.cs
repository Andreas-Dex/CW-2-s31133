using System;
using System.Collections.Generic;
using System.Linq;

interface IHazardNotifier
{
    void NotifyHazard(string message);
}

class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

abstract class Container
{
    private static int counter = 1;
    public string SerialNumber { get; private set; }
    public double MaxCapacity { get; protected set; }
    public double Height { get; protected set; }
    public double Depth { get; protected set; }
    public double OwnWeight { get; protected set; }
    public double Load { get; protected set; }

    public Container(string type, double maxCapacity, double height, double depth, double ownWeight)
    {
        SerialNumber = $"KON-{type}-{counter++}";
        MaxCapacity = maxCapacity;
        Height = height;
        Depth = depth;
        OwnWeight = ownWeight;
    }

    public virtual void LoadCargo(double mass)
    {
        if (mass > MaxCapacity)
        {
            throw new OverfillException($"Przekroczono pojemnosc kontenera {SerialNumber}! Max: {MaxCapacity}");
        }
        Load = mass;
    }

    public virtual void Unload()
    {
        Load = 0;
    }
}

class LiquidContainer : Container, IHazardNotifier
{
    private bool isHazardous;

    public LiquidContainer(double maxCapacity, bool isHazardous, double height, double depth, double ownWeight)
        : base("L", maxCapacity, height, depth, ownWeight)
    {
        this.isHazardous = isHazardous;
    }

    public override void LoadCargo(double mass)
    {
        double limit = isHazardous ? MaxCapacity * 0.5 : MaxCapacity * 0.9;
        if (mass > limit)
        {
            NotifyHazard($"Proba zaladunku niebezpiecznego ladunku ponad limit! [{SerialNumber}]");
            throw new OverfillException("Zbyt duzy ladunek!");
        }
        Load = mass;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine("HAZARD: " + message);
    }
}

class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; private set; }

    public GasContainer(double maxCapacity, double pressure, double height, double depth, double ownWeight)
        : base("G", maxCapacity, height, depth, ownWeight)
    {
        Pressure = pressure;
    }

    public override void LoadCargo(double mass)
    {
        if (mass > MaxCapacity)
        {
            NotifyHazard($"Przekroczona ladownosc gazu! [{SerialNumber}]");
            throw new OverfillException("Zbyt duzy ladunek!");
        }
        Load = mass;
    }

    public override void Unload()
    {
        Load *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine("HAZARD: " + message);
    }
}

class RefrigeratedContainer : Container
{
    public string ProductType { get; private set; }
    public double Temperature { get; private set; }

    public RefrigeratedContainer(double maxCapacity, string productType, double temperature, double height, double depth, double ownWeight)
        : base("C", maxCapacity, height, depth, ownWeight)
    {
        ProductType = productType;
        Temperature = temperature;
    }

    public override void LoadCargo(double mass)
    {
        if (mass > MaxCapacity)
        {
            throw new OverfillException("Zbyt duzy ladunek!");
        }
        Load = mass;
    }
}

class Ship
{
    public string Name { get; private set; }
    public int MaxContainerCount { get; private set; }
    public double MaxWeight { get; private set; }
    public double Speed { get; private set; }
    private List<Container> containers = new List<Container>();

    public Ship(string name, int maxContainerCount, double maxWeight, double speed)
    {
        Name = name;
        MaxContainerCount = maxContainerCount;
        MaxWeight = maxWeight;
        Speed = speed;
    }

    public bool AddContainer(Container container)
    {
        if (containers.Count >= MaxContainerCount)
        {
            Console.WriteLine("Za duzo kontenerow!");
            return false;
        }

        double currentWeight = containers.Sum(c => c.Load + c.OwnWeight);
        double newWeight = currentWeight + container.Load + container.OwnWeight;

        if (newWeight > MaxWeight)
        {
            Console.WriteLine("Statek przeciazony!");
            return false;
        }

        containers.Add(container);
        return true;
    }

    public void RemoveContainer(string serial)
    {
        containers.RemoveAll(c => c.SerialNumber == serial);
    }

    public void ReplaceContainer(string serial, Container newContainer)
    {
        RemoveContainer(serial);
        AddContainer(newContainer);
    }

    public void MoveContainerTo(string serial, Ship targetShip)
    {
        Container found = containers.FirstOrDefault(c => c.SerialNumber == serial);
        if (found != null && targetShip.AddContainer(found))
        {
            RemoveContainer(serial);
        }
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Statek: {Name} | Predkosc: {Speed} | Max kontenery: {MaxContainerCount} | Max waga: {MaxWeight}");
        foreach (var c in containers)
        {
            Console.WriteLine($"- {c.SerialNumber} (ladunek: {c.Load})");
        }
    }
}

class Program
{
    static void Main()
    {
        Ship ship1 = new Ship("Statek1", 5, 50000, 25);

        Container c1 = new LiquidContainer(10000, true, 250, 300, 3000);
        Container c2 = new RefrigeratedContainer(12000, "banany", -5, 260, 310, 4000);
        Container c3 = new GasContainer(8000, 10, 240, 290, 2500);

        try
        {
            c1.LoadCargo(4000);
            c2.LoadCargo(11000);
            c3.LoadCargo(7000);
        }
        catch (OverfillException e)
        {
            Console.WriteLine("BLAD: " + e.Message);
        }

        ship1.AddContainer(c1);
        ship1.AddContainer(c2);
        ship1.AddContainer(c3);

        ship1.PrintInfo();
    }
}