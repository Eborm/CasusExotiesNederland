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

                command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Locatie (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Land TEXT NOT NULL,
                            Breedtegraad REAL,
                            Lengtegraad REAL,
                            OrganismeId INTEGER NOT NULL,
                            FOREIGN KEY (OrganismeId) REFERENCES Organismen (Id)
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

            // Voeg het organisme toe
            if (organisme is Dier dier)
            {
                command.CommandText = @"
                INSERT INTO Organismen (Naam, Type, Oorsprong, Leefgebied, beschrijving)
                VALUES (@naam, @type, @oorsprong, @leefgebied, @beschrijving);
                SELECT last_insert_rowid();";

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
                SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@naam", plant.Naam);
                command.Parameters.AddWithValue("@type", "plant");
                command.Parameters.AddWithValue("@oorsprong", plant.Oorsprong);
                command.Parameters.AddWithValue("@hoogteInMeters", plant.HoogteInMeters);
                command.Parameters.AddWithValue("@beschrijving", plant.beschrijving);
            }

            // Haal het ID van het toegevoegde organisme op
            var organismeId = (long)command.ExecuteScalar();

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

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Organismen LEFT JOIN Locatie on Organismen.Id = Locatie.Id";

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
                            Beschrijving = Beschrijving = reader.IsDBNull(reader.GetOrdinal("Beschrijving")) ? null : reader.GetString(reader.GetOrdinal("Beschrijving"));

                        Dier tempDier = new Dier(Naam, Oorsprong, Leefgebied,
                            reader.GetString(reader.GetOrdinal("Land")),
                            reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                            reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                            Beschrijving);
                            organismen.Add(tempDier);
                        }
                        else
                        {
                            Beschrijving = Beschrijving = reader.IsDBNull(reader.GetOrdinal("Beschrijving")) ? null : reader.GetString(reader.GetOrdinal("Beschrijving"));
                            Naam = reader.GetString(reader.GetOrdinal("Naam"));
                            Oorsprong = reader.GetString(reader.GetOrdinal("Oorsprong"));
                            HoogteInMeters = reader.GetDouble(reader.GetOrdinal("HoogteInMeters"));
                        Plant tempPlant = new Plant(Naam, Oorsprong, HoogteInMeters,
                            reader.GetString(reader.GetOrdinal("Land")),
                            reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                            reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                            Beschrijving);
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
                command.CommandText = "SELECT * FROM Organismen LEFT JOIN Locatie on Organismen.Id = Locatie.Id WHERE Type = @type";
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
                                reader.GetString(reader.GetOrdinal("Beschrijving"))
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
                                reader.GetString(reader.GetOrdinal("Beschrijving"))
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
                command.CommandText = @"SELECT * FROM Organismen JOIN Locatie ON Organismen.Id = Locatie.Id WHERE Oorsprong = @oorsprong";
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
                                reader.GetString(reader.GetOrdinal("Beschrijving"))
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
                                reader.GetString(reader.GetOrdinal("Beschrijving"))
                            ));
                        }
                    }
                }
            }

            return organismen;
        }
}
