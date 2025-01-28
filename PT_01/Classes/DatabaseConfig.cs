using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;
using MySql.Data.MySqlClient;

public class Databaseconfig
{
    public static string GetConnectionString()
    {
        return "Server=172.211.6.172;" +
               "Database=exotischnederlanddb;" +
               "User ID=ssadmin;" +
               "Password=Welkom1235!;";
    }
}