using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Data;

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
        string Naam = "";
        string Oorsprong = "";
        string Leefgebied = "";
        double HoogteInMeters = 0.0;

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
                    if (type == "dier")
                    {
                        Naam = reader.GetString(reader.GetOrdinal("Naam"));
                        Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong"));
                        Leefgebied = reader.IsDBNull(reader.GetOrdinal("Leefgebied")) ? null : reader.GetString(reader.GetOrdinal("Leefgebied"));
                        Dier tempDier = new Dier(Naam, Oorsprong, Leefgebied);
                        organismen.Add(tempDier);
                    }
                    else
                    {
                        Naam = reader.GetString(reader.GetOrdinal("Naam"));
                        Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong"));
                        HoogteInMeters = reader.GetDouble(reader.GetOrdinal("HoogteInMeters"));
                        Plant tempPlant = new Plant(Naam, Oorsprong, HoogteInMeters);
                        organismen.Add(tempPlant);
                    }
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
                    if (type == "dier")
                    {
                        organismen.Add(new Dier(
                            reader.GetString(reader.GetOrdinal("Naam")),
                            reader.GetString(reader.GetOrdinal("Oorsprong")),
                            reader.IsDBNull(reader.GetOrdinal("Leefgebied")) ? null : reader.GetString(reader.GetOrdinal("Leefgebied"))
                        ));
                    }
                    else if (type == "plant")
                    {
                        organismen.Add(new Plant(
                            reader.GetString(reader.GetOrdinal("Naam")),
                            reader.GetString(reader.GetOrdinal("Oorsprong")),
                            reader.IsDBNull(reader.GetOrdinal("HoogteInMeters")) ? 0 : reader.GetDouble(reader.GetOrdinal("HoogteInMeters"))
                        ));
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
                    if (type == "dier")
                    {
                        organismen.Add(new Dier(
                            reader.GetString(reader.GetOrdinal("Naam")),
                            reader.GetString(reader.GetOrdinal("Oorsprong")),
                            reader.IsDBNull(reader.GetOrdinal("Leefgebied")) ? null : reader.GetString(reader.GetOrdinal("Leefgebied"))
                        ));
                    }
                    else if (type == "plant")
                    {
                        organismen.Add(new Plant(
                            reader.GetString(reader.GetOrdinal("Naam")),
                            reader.GetString(reader.GetOrdinal("Oorsprong")),
                            reader.IsDBNull(reader.GetOrdinal("HoogteInMeters")) ? 0 : reader.GetDouble(reader.GetOrdinal("HoogteInMeters"))
                        ));
                    }
                }
            }
        }

        return organismen;
    }
}
