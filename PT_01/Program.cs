using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Data;

// Aangepaste Program klasse
class Program
{
    private static DatabaseService _databaseService;

    static void Main(string[] args)
    {
        // Creëren van het pad waar de database is opgeslagen
        string? projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\"));
        string? databasePath = Path.Combine(projectRoot, "Database", "organismen.db");
        // Debug Console.WriteLine(databasePath);



        // Initialiseer de database service
        _databaseService = new DatabaseService(databasePath);

        bool doorgaan = true;
        while (doorgaan)
        {
            Console.Clear();
            ToonMenu();
            string keuze = Console.ReadLine();

            switch (keuze)
            {
                case "1":
                    VoegOrganismeToe();
                    break;
                case "2":
                    ToonAlleOrganismen();
                    break;
                case "3":
                    FilterOpType();
                    break;
                case "4":
                    FilterOpOorsprong();
                    break;
                case "5":
                    doorgaan = false;
                    break;
                default:
                    Console.WriteLine("Ongeldige keuze. Druk op een toets om door te gaan.");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private static void ToonMenu()
    {
        Console.WriteLine("Kies een optie:");
        Console.WriteLine("1. Nieuw organisme toevoegen");
        Console.WriteLine("2. Alle organismen bekijken");
        Console.WriteLine("3. Filteren op type (Dier/Plant)");
        Console.WriteLine("4. Filteren op oorsprong (Inheems/Exoot)");
        Console.WriteLine("5. Afsluiten");
    }

    private static void VoegOrganismeToe()
    {
        Console.WriteLine("Is het een dier of plant?");
        string type = Console.ReadLine();

        if (type.ToLower() == "dier")
        {
            var dier = new Dier();
            Console.Write("Voer de naam in: ");
            dier.Naam = Console.ReadLine();

            Console.Write("Is het inheems of exotisch? ");
            dier.Oorsprong = Console.ReadLine();

            Console.Write("Wat is het leefgebied? ");
            dier.Leefgebied = Console.ReadLine();

            _databaseService.VoegOrganismeToe(dier);
        }
        else if (type.ToLower() == "plant")
        {
            var plant = new Plant();
            Console.Write("Voer de naam in: ");
            plant.Naam = Console.ReadLine();

            Console.Write("Is het inheems of exotisch? ");
            plant.Oorsprong = Console.ReadLine();

            Console.Write("Wat is de hoogte in meters? ");
            if (double.TryParse(Console.ReadLine(), out double hoogte))
            {
                plant.HoogteInMeters = hoogte;
            }

            _databaseService.VoegOrganismeToe(plant);
        }

        Console.WriteLine("\nOrganisme toegevoegd! Druk op een toets om door te gaan.");
        Console.ReadKey();
    }

    private static void ToonAlleOrganismen()
    {
        var organismen = _databaseService.HaalAlleOrganismenOp();

        if (!organismen.Any())
        {
            Console.WriteLine("Geen organismen gevonden.");
        }
        else
        {
            foreach (var organisme in organismen)
            {
                Console.WriteLine(organisme.Beschrijving());
            }
        }

        Console.WriteLine("\nDruk op een toets om door te gaan.");
        Console.ReadKey();
    }

    private static void FilterOpType()
    {
        Console.Write("Welk type wil je zien (Dier/Plant)? ");
        string type = Console.ReadLine();

        var gefilterd = _databaseService.FilterOpType(type);

        foreach (var organisme in gefilterd)
        {
            Console.WriteLine(organisme.Beschrijving());
        }

        Console.WriteLine("\nDruk op een toets om door te gaan.");
        Console.ReadKey();
    }

    private static void FilterOpOorsprong()
    {
        Console.Write("Welke oorsprong wil je zien (Inheems/Exoot)? ");
        string oorsprong = Console.ReadLine();

        var gefilterd = _databaseService.FilterOpOorsprong(oorsprong);

        foreach (var organisme in gefilterd)
        {
            Console.WriteLine(organisme.Beschrijving());
        }

        Console.WriteLine("\nDruk op een toets om door te gaan.");
        Console.ReadKey();
    }
}