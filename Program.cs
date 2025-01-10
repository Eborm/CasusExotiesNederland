using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using System.Data;

public class Organisme
{
    public string Naam { get; set; }
    public string Type { get; set; }
    public string Oorsprong { get; set; }

    public virtual string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()}.";
    }
}

// Subklasse voor dieren
public class Dier : Organisme
{
    public string Leefgebied { get; set; }

    public Dier()
    {
        Type = "Dier";
    }

    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en leeft in {Leefgebied}.";
    }
}

// Subklasse voor planten
public class Plant : Organisme
{
    public double HoogteInMeters { get; set; }

    public Plant()
    {
        Type = "Plant";
    }

    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en kan {HoogteInMeters} meter hoog worden.";
    }
}
// Database Service class voor alle database operaties
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Maak de organismen tabel
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Organismen (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Naam TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Oorsprong TEXT NOT NULL,
                    Leefgebied TEXT,
                    HoogteInMeters REAL
                )";
            command.ExecuteNonQuery();
        }
    }

    public void VoegOrganismeToe(Organisme organisme)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

            if (organisme is Dier dier)
            {
                command.CommandText = @"
                    INSERT INTO Organismen (Naam, Type, Oorsprong, Leefgebied)
                    VALUES (@naam, @type, @oorsprong, @leefgebied)";
                command.Parameters.AddWithValue("@leefgebied", dier.Leefgebied);
            }
            else if (organisme is Plant plant)
            {
                command.CommandText = @"
                    INSERT INTO Organismen (Naam, Type, Oorsprong, HoogteInMeters)
                    VALUES (@naam, @type, @oorsprong, @hoogteInMeters)";
                command.Parameters.AddWithValue("@hoogteInMeters", plant.HoogteInMeters);
            }

            command.Parameters.AddWithValue("@naam", organisme.Naam);
            command.Parameters.AddWithValue("@type", organisme.Type);
            command.Parameters.AddWithValue("@oorsprong", organisme.Oorsprong);

            command.ExecuteNonQuery();
        }
    }

    public List<Organisme> HaalAlleOrganismenOp()
    {
        var organismen = new List<Organisme>();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Organismen";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(reader.GetOrdinal("Type"));
                    Organisme organisme;

                    if (type == "Dier")
                    {
                        organisme = new Dier
                        {
                            Naam = reader.GetString(reader.GetOrdinal("Naam")),
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong")),
                            Leefgebied = reader.IsDBNull(reader.GetOrdinal("Leefgebied"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Leefgebied"))
                        };
                    }
                    else
                    {
                        organisme = new Plant
                        {
                            Naam = reader.GetString(reader.GetOrdinal("Naam")),
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong")),
                            HoogteInMeters = reader.IsDBNull(reader.GetOrdinal("HoogteInMeters"))
                                ? 0
                                : reader.GetDouble(reader.GetOrdinal("HoogteInMeters"))
                        };
                    }

                    organismen.Add(organisme);
                }
            }
        }

        return organismen;
    }

    public List<Organisme> FilterOpType(string type)
    {
        var organismen = new List<Organisme>();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Organismen WHERE Type = @type";
            command.Parameters.AddWithValue("@type", type);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (type == "Dier")
                    {
                        organismen.Add(new Dier
                        {
                            Naam = reader.GetString(reader.GetOrdinal("Naam")),
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong")),
                            Leefgebied = reader.GetString(reader.GetOrdinal("Leefgebied"))
                        });
                    }
                    else
                    {
                        organismen.Add(new Plant
                        {
                            Naam = reader.GetString(reader.GetOrdinal("Naam")),
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong")),
                            HoogteInMeters = reader.GetDouble(reader.GetOrdinal("HoogteInMeters"))
                        });
                    }
                }
            }
        }

        return organismen;
    }

    public List<Organisme> FilterOpOorsprong(string oorsprong)
    {
        var organismen = new List<Organisme>();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Organismen WHERE Oorsprong = @oorsprong";
            command.Parameters.AddWithValue("@oorsprong", oorsprong);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(reader.GetOrdinal("Type"));
                    if (type == "Dier")
                    {
                        organismen.Add(new Dier
                        {
                            Naam = reader.GetString(reader.GetOrdinal("Naam")),
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong")),
                            Leefgebied = reader.GetString(reader.GetOrdinal("Leefgebied"))
                        });
                    }
                    else
                    {
                        organismen.Add(new Plant
                        {
                            Naam = reader.GetString(reader.GetOrdinal("Naam")),
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong")),
                            HoogteInMeters = reader.GetDouble(reader.GetOrdinal("HoogteInMeters"))
                        });
                    }
                }
            }
        }

        return organismen;
    }
}

// Aangepaste Program klasse
class Program
{
    private static DatabaseService _databaseService;

    static void Main(string[] args)
    {
        // Initialiseer de database service
        _databaseService = new DatabaseService("organismen.db");

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