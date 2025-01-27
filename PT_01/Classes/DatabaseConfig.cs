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
        return "Server=ssdatabase.bywf3uhkkvdetepfj3egbxwzbf.ax.internal.cloudapp.net;" +
               "Database=exotischnederlanddb;" +
               "User ID=ssadmin;" +
               "Password=Welkom012345!;";
    }
}