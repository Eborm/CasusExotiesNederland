using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


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
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();

            var commands = new[]
            {
                @"CREATE TABLE IF NOT EXISTS Organismen (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Naam VARCHAR(100) NOT NULL,
                    Type VARCHAR(100) NOT NULL,
                    Oorsprong VARCHAR(100) NOT NULL,
                    Leefgebied VARCHAR(100),
                    HoogteInMeters FLOAT,
                    Beschrijving VARCHAR(100)
                )",
                @"CREATE TABLE IF NOT EXISTS Locatie (
                    Id INT PRIMARY KEY,
                    Land VARCHAR(100),
                    Breedtegraad FLOAT,
                    Lengtegraad FLOAT,
                    FOREIGN KEY (Id) REFERENCES Organismen(Id)
                )",
                @"CREATE TABLE IF NOT EXISTS DatumTijd (
                    Id INT PRIMARY KEY,
                    Tijd VARCHAR(100),
                    Datum VARCHAR(100),
                    FOREIGN KEY (Id) REFERENCES Organismen(Id)
                )"
            };

            foreach (var commandText in commands)
            {
                using (var command = new MySqlCommand(commandText, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public void VoegOrganismeToe(Organisme organisme)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

            if (organisme is Dier dier)
            {
                command.CommandText = @"
                INSERT INTO Organismen (Naam, Type, Oorsprong, Leefgebied, Beschrijving)
                VALUES (@naam, @type, @oorsprong, @leefgebied, @beschrijving);
                SELECT LAST_INSERT_ID();";

                command.Parameters.AddWithValue("@naam", dier.Naam);
                command.Parameters.AddWithValue("@type", "dier");
                command.Parameters.AddWithValue("@oorsprong", dier.Oorsprong);
                command.Parameters.AddWithValue("@leefgebied", dier.Leefgebied ?? DBNull.Value.ToString());
                command.Parameters.AddWithValue("@beschrijving", dier.Beschrijving() ?? DBNull.Value.ToString());
            }
            else if (organisme is Plant plant)
            {
                command.CommandText = @"
                INSERT INTO Organismen (Naam, Type, Oorsprong, HoogteInMeters, Beschrijving)
                VALUES (@naam, @type, @oorsprong, @hoogteInMeters, @beschrijving);
                SELECT LAST_INSERT_ID();";

                command.Parameters.AddWithValue("@naam", plant.Naam);
                command.Parameters.AddWithValue("@type", "plant");
                command.Parameters.AddWithValue("@oorsprong", plant.Oorsprong);
                command.Parameters.AddWithValue("@hoogteInMeters", plant.HoogteInMeters);
                command.Parameters.AddWithValue("@beschrijving", plant.Beschrijving() ?? DBNull.Value.ToString());
            }

            var organismeId = Convert.ToInt32(command.ExecuteScalar());

            if (!string.IsNullOrEmpty(organisme.Land))
            {
                using (var locatieCommand = connection.CreateCommand())
                {
                    locatieCommand.CommandText = @"
                    INSERT INTO Locatie (Id, Land, Breedtegraad, Lengtegraad)
                    VALUES (@Id, @Land, @Breedtegraad, @Lengtegraad)";
                    locatieCommand.Parameters.AddWithValue("@Id", organismeId);
                    locatieCommand.Parameters.AddWithValue("@Land", organisme.Land);
                    locatieCommand.Parameters.AddWithValue("@Breedtegraad", organisme.Breedtegraad);
                    locatieCommand.Parameters.AddWithValue("@Lengtegraad", organisme.Lengtegraad);
                    locatieCommand.ExecuteNonQuery();
                }

                using (var datumCommand = connection.CreateCommand())
                {
                    datumCommand.CommandText = @"
                    INSERT INTO DatumTijd (Id, Tijd, Datum)
                    VALUES (@Id, @Tijd, @Datum)";
                    datumCommand.Parameters.AddWithValue("@Id", organismeId);
                    datumCommand.Parameters.AddWithValue("@Tijd", organisme.tijd ?? DBNull.Value.ToString());
                    datumCommand.Parameters.AddWithValue("@Datum", organisme.datum ?? DBNull.Value.ToString());
                    datumCommand.ExecuteNonQuery();
                }
            }
        }
    }

    public List<Organisme> HaalAlleOrganismenOp()
    {
        var organismen = new List<Organisme>();

        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Organismen
                LEFT JOIN Locatie ON Organismen.Id = Locatie.Id
                LEFT JOIN DatumTijd ON Organismen.Id = DatumTijd.Id";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(reader.GetOrdinal("Type"));

                    if (type == "dier")
                    {
                        var dier = new Dier(
                            reader["Naam"].ToString(),
                            reader["Oorsprong"].ToString(),
                            reader["Leefgebied"]?.ToString(),
                            reader["Land"]?.ToString(),
                            reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                            reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                            reader["Beschrijving"]?.ToString(),
                            reader["Tijd"]?.ToString(),
                            reader["Datum"]?.ToString()
                        );
                        organismen.Add(dier);
                    }
                    else if (type == "plant")
                    {
                        var plant = new Plant(
                            reader["Naam"].ToString(),
                            reader["Oorsprong"].ToString(),
                            reader.IsDBNull(reader.GetOrdinal("HoogteInMeters")) ? 0 : reader.GetDouble(reader.GetOrdinal("HoogteInMeters")),
                            reader["Land"]?.ToString(),
                            reader.IsDBNull(reader.GetOrdinal("Breedtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Breedtegraad")),
                            reader.IsDBNull(reader.GetOrdinal("Lengtegraad")) ? 0 : reader.GetDouble(reader.GetOrdinal("Lengtegraad")),
                            reader["Beschrijving"]?.ToString(),
                            reader["Tijd"]?.ToString(),
                            reader["Datum"]?.ToString()
                        );
                        organismen.Add(plant);
                    }
                }
            }
        }

        return organismen;
    }
}