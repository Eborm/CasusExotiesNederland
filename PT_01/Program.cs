using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Identity.Client;

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
        string Naam = "";
        string Oorsprong = "";
        string type = "";
        bool ongeldigeinvoer = true;
        while (ongeldigeinvoer)
        {
            Console.WriteLine("Is het een dier of plant?");
            type = Console.ReadLine().ToLower();

            if (type == "dier" || type == "plant") { ongeldigeinvoer = false; }
            else { Console.WriteLine("Voer astublief dier of plant in."); }
        }
        ongeldigeinvoer = true;
        Console.Write("Voer de naam in: ");
        Naam = Console.ReadLine();
        while (ongeldigeinvoer)
        {
            Console.Write("Is het inheems of exotisch? ");
            Oorsprong = Console.ReadLine().ToLower();
            if (Oorsprong == "inheems" || Oorsprong == "exotisch") { ongeldigeinvoer = false; }
            else { Console.WriteLine("Ongeldige invoer."); }
        }
        ongeldigeinvoer = true;

        if (type == "dier")
        {


            Console.Write("Wat is het leefgebied? ");
            string Leefgebied = Console.ReadLine();

            var dier = new Dier(Naam, Oorsprong, Leefgebied);
            _databaseService.VoegOrganismeToe(dier);
        }
        else
        {
            while (ongeldigeinvoer)
            {
                Console.Write("Wat is de hoogte in meters? ");
                string hoogteString = Console.ReadLine();
                string tempstring = hoogteString;
                
                List<string> templist = tempstring.Split(",").ToList();
                for (int i = 0; i < templist.Count; i++)
                {
                    List<string> templist2 = templist[i].Split(".").ToList();
                    templist2.Reverse();
                    templist.Remove(templist[i]);
                    for (int j = 0; j < templist2.Count; j++)
                    {
                        templist.Add(templist2[j]);
                    }
                }
                templist.Reverse();
                hoogteString = string.Join(",", templist);

                if (double.TryParse(hoogteString, out double hoogte))
                {
                    var plant = new Plant(Naam, Oorsprong, hoogte);
                    ongeldigeinvoer = false;
                    _databaseService.VoegOrganismeToe(plant);
                }
                else { Console.WriteLine("Voer astublieft een punt in inplaats van een comma"); }
            }
            ongeldigeinvoer = true;
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
        string type = Console.ReadLine().ToLower();

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
        bool ongeldigeinvoer = true;
        string oorsprong = "";
        while (ongeldigeinvoer)
        {
            Console.Write("Welke oorsprong wil je zien (Inheems/Exoot)? ");
            oorsprong = Console.ReadLine().ToLower();
            if (oorsprong == "inheems" || oorsprong == "exoot")
            {
                ongeldigeinvoer = false;
                if (oorsprong == "exooot") { oorsprong = "exotisch"; }
            }
            else { Console.WriteLine("Ongeldige invoer"); }
        }

        var gefilterd = _databaseService.FilterOpOorsprong(oorsprong);

        foreach (var organisme in gefilterd)
        {
            Console.WriteLine(organisme.Beschrijving());
        }

        Console.WriteLine("\nDruk op een toets om door te gaan.");
        Console.ReadKey();
    }
}