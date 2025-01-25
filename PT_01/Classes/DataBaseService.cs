using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;

    // Database Service class voor alle database operaties
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Maak de organismen tabel
            var commands = new[]
            {
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Organismen' AND xtype='U')
                BEGIN
                    CREATE TABLE Organismen (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        Naam NVARCHAR(100) NOT NULL,
                        Type NVARCHAR(100) NOT NULL,
                        Oorsprong NVARCHAR(100) NOT NULL,
                        Leefgebied NVARCHAR(100),
                        HoogteInMeters FLOAT
                    )
                END",

                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Locatie' AND xtype='U')
                BEGIN
                    CREATE TABLE Locatie (
                        Id INT PRIMARY KEY,
                        Land NVARCHAR(100),
                        Breedtegraad FLOAT,
                        Lengtegraad FLOAT,
                        FOREIGN KEY (Id) REFERENCES Organismen(Id)
                    )
                END",

                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DatumTijd' AND xtype='U')
                BEGIN
                    CREATE TABLE DatumTijd (
                        Id INT PRIMARY KEY,
                        Tijd NVARCHAR(100),
                        Datum NVARCHAR(100),
                        FOREIGN KEY (Id) REFERENCES Organismen(Id)
                    )
                END"
            };

                foreach (var commandText in commands)
                {
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void VoegOrganismeToe(Organisme organisme)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Voeg het organisme toe
                if (organisme is Dier dier)
                {
                    command.CommandText = @"
                    INSERT INTO Organismen (Naam, Type, Oorsprong, Leefgebied, Beschrijving)
                    VALUES (@naam, @type, @oorsprong, @leefgebied, @beschrijving);
                    SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@naam", dier.Naam);
                    command.Parameters.AddWithValue("@type", "dier");
                    command.Parameters.AddWithValue("@oorsprong", dier.Oorsprong);
                    command.Parameters.AddWithValue("@leefgebied", dier.Leefgebied);
                    command.Parameters.AddWithValue("@beschrijving", dier.beschrijving);
                }
                else if (organisme is Plant plant)
                {
                    command.CommandText = @"
                    INSERT INTO Organismen (Naam, Type, Oorsprong, HoogteInMeters, Beschrijving)
                    VALUES (@naam, @type, @oorsprong, @hoogteInMeters, @beschrijving);
                    SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@naam", plant.Naam);
                    command.Parameters.AddWithValue("@type", "plant");
                    command.Parameters.AddWithValue("@oorsprong", plant.Oorsprong);
                    command.Parameters.AddWithValue("@hoogteInMeters", plant.HoogteInMeters);
                    command.Parameters.AddWithValue("@beschrijving", plant.beschrijving);
                }

                // Haal het ID van het toegevoegde organisme op
                var organismeId = (long)(decimal)command.ExecuteScalar();

                // Voeg de locatie toe
                if (organisme.Land != null)
                {
                    var locatieCommand = connection.CreateCommand();
                    locatieCommand.CommandText = @"
                    INSERT INTO Locatie (Id, Land, Breedtegraad, Lengtegraad)
                    VALUES (@Id, @Land, @Breedtegraad, @Lengtegraad)";

                    locatieCommand.Parameters.AddWithValue("@Land", organisme.Land);
                    locatieCommand.Parameters.AddWithValue("@Breedtegraad", organisme.Breedtegraad);
                    locatieCommand.Parameters.AddWithValue("@Lengtegraad", organisme.Lengtegraad);
                    locatieCommand.Parameters.AddWithValue("@Id", organismeId);

                    locatieCommand.ExecuteNonQuery();

                    var DatumCommand = connection.CreateCommand();
                    DatumCommand.CommandText = @"
                    INSERT INTO DatumTijd (Id, Tijd, Datum)
                    VALUES (@Id, @Tijd, @Datum)";

                    DatumCommand.Parameters.AddWithValue("@Tijd", organisme.tijd);
                    DatumCommand.Parameters.AddWithValue("@Datum", organisme.datum);
                    DatumCommand.Parameters.AddWithValue("@Id", organismeId);

                    DatumCommand.ExecuteNonQuery();

                }
            }
        }

        public List<Organisme> HaalAlleOrganismenOp()
        {
            var organismen = new List<Organisme>();
            string Naam = "";
            string Oorsprong = "";
            string Leefgebied = "";
            double HoogteInMeters = 0.0;
            string Beschrijving = "";
            string Tijd = "";
            string Datum = "";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Organismen LEFT JOIN Locatie on Organismen.Id = Locatie.Id LEFT JOIN DatumTijd on Organismen.Id = DatumTijd.Id";

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
                            Beschrijving = reader.IsDBNull(reader.GetOrdinal("Beschrijving")) ? null : reader.GetString(reader.GetOrdinal("Beschrijving"));
                            Tijd = reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd"));
                            Datum = reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"));

                            Dier tempDier = new Dier(Naam, Oorsprong, Leefgebied,
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                Beschrijving, Tijd, Datum);
                            organismen.Add(tempDier);
                        }
                        else
                        {
                            Tijd = reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd"));
                            Datum = reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"));
                            Beschrijving = Beschrijving = reader.IsDBNull(reader.GetOrdinal("Beschrijving")) ? null : reader.GetString(reader.GetOrdinal("Beschrijving"));
                            Naam = reader.GetString(reader.GetOrdinal("Naam"));
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong"));
                            HoogteInMeters = reader.GetDouble(reader.GetOrdinal("HoogteInMeters"));
                            Plant tempPlant = new Plant(Naam, Oorsprong, HoogteInMeters,
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                Beschrijving, Tijd, Datum);
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

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Organismen LEFT JOIN Locatie on Organismen.Id = Locatie.Id JOIN DatumTijd ON Organismen.Id = DatumTijd.Id WHERE Type = @type";
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
                                reader.IsDBNull(reader.GetOrdinal("Leefgebied")) ? null : reader.GetString(reader.GetOrdinal("Leefgebied")),
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                reader.GetString(reader.GetOrdinal("Beschrijving")),
                                reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd")),
                                reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"))
                            ));
                        }
                        else if (type == "plant")
                        {
                            organismen.Add(new Plant(
                                reader.GetString(reader.GetOrdinal("Naam")),
                                reader.GetString(reader.GetOrdinal("Oorsprong")),
                                reader.IsDBNull(reader.GetOrdinal("HoogteInMeters")) ? 0 : reader.GetDouble(reader.GetOrdinal("HoogteInMeters")),
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                reader.GetString(reader.GetOrdinal("Beschrijving")),
                                reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd")),
                                reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"))
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

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM Organismen JOIN Locatie ON Organismen.Id = Locatie.Id JOIN DatumTijd ON Organismen.Id = DatumTijd.Id WHERE Oorsprong = @oorsprong";
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
                                reader.IsDBNull(reader.GetOrdinal("Leefgebied")) ? null : reader.GetString(reader.GetOrdinal("Leefgebied")),
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                reader.GetString(reader.GetOrdinal("Beschrijving")),
                                reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd")),
                                reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"))
                            ));
                        }
                        else if (type == "plant")
                        {
                            organismen.Add(new Plant(
                                reader.GetString(reader.GetOrdinal("Naam")),
                                reader.GetString(reader.GetOrdinal("Oorsprong")),
                                reader.IsDBNull(reader.GetOrdinal("HoogteInMeters")) ? 0 : reader.GetDouble(reader.GetOrdinal("HoogteInMeters")),
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                reader.GetString(reader.GetOrdinal("Beschrijving")),
                                reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd")),
                                reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"))
                            ));
                        }
                    }
                }
            }

            return organismen;
        }

        public List<Organisme> FilterenOpAlfabetische()
        {
            var organismen = new List<Organisme>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM Organismen JOIN Locatie ON Organismen.Id = Locatie.Id JOIN DatumTijd ON Organismen.Id = DatumTijd.Id ORDER BY Naam";

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
                                reader.IsDBNull(reader.GetOrdinal("Leefgebied")) ? null : reader.GetString(reader.GetOrdinal("Leefgebied")),
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                reader.GetString(reader.GetOrdinal("Beschrijving")),
                                reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd")),
                                reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"))
                            ));
                        }
                        else if (type == "plant")
                        {
                            organismen.Add(new Plant(
                                reader.GetString(reader.GetOrdinal("Naam")),
                                reader.GetString(reader.GetOrdinal("Oorsprong")),
                                reader.IsDBNull(reader.GetOrdinal("HoogteInMeters")) ? 0 : reader.GetDouble(reader.GetOrdinal("HoogteInMeters")),
                                reader.GetString(reader.GetOrdinal("Land")),
                                reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                                reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                                reader.GetString(reader.GetOrdinal("Beschrijving")),
                                reader.IsDBNull(reader.GetOrdinal("Tijd")) ? null : reader.GetString(reader.GetOrdinal("Tijd")),
                                reader.IsDBNull(reader.GetOrdinal("Datum")) ? null : reader.GetString(reader.GetOrdinal("Datum"))
                            ));
                        }
                    }
                }
            }

            return organismen;
        }
    }
