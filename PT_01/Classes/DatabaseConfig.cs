using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;

public class Databaseconfig
{
    public static string GetConnectionString()
    {
        return "Server=casusexotischnederland.database.windows.net;" +
               "Database=excotischnederlanddb;" +
               "User Id=adminchris;" +
               "Password=Welkom012345!;" +
               "Encrypt=True;";
    }
}