using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Http.Headers;
using System.Linq.Expressions;

// Aangepaste Program klasse
class Program
{
    private static DatabaseService _databaseService;

    static async Task Main(string[] args)
    {
        // Creëren van het pad waar de database is opgeslagen
        string? projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\"));
        string? databasePath = Path.Combine(projectRoot, "Database", "organismen.db");

        // Initialiseer de database service
        _databaseService = new DatabaseService(Databaseconfig.GetConnectionString());

        bool doorgaan = true;
        while (doorgaan)
        {
            Console.Clear();
            ToonMenu();
            string keuze = Console.ReadLine();

            switch (keuze)
            {
                case "1":
                    await VoegOrganismeToe();
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
                    VolledigeBeschrijving();
                    break;
                case "6":
                    SortAlpahbetisch();
                    break;
                case "7":
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
        Console.WriteLine("5. Volledige beschrijving tonen");
        Console.WriteLine("6. Sorteren op alphabetische volgoorde");
        Console.WriteLine("7. Afsluiten");
    }

    private static async Task<string> ToonLocatieViaIP()
    {
        string apiUrl = "https://ipwhois.app/json/";

        using HttpClient client = new HttpClient();

        try
        {
            // Maak een GET-aanroep naar de gratis API
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            // Lees de JSON-respons
            string responseBody = await response.Content.ReadAsStringAsync();

            // JSON-parser
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

            // Haal de gewenste gegevens op
            string country = data.country;
            string latitude = data.latitude;
            string longitude = data.longitude;

            string datastring = country.ToString() + "," + latitude.ToString() + "," + longitude.ToString();
            return datastring;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij het ophalen van locatie: {ex.Message}");
            return "fail";
        }
    }

    private static async Task VoegOrganismeToe()
    {
        string Data = await ToonLocatieViaIP();

        string Land = "";
        double Breedtegraad = 0.000;
        double Lengtegraad = 0.000;
        string tijd = "25:65:65";
        string datum = "-01/-01/-01";
        if (Data != "fail")
        {
            List<string> DataList = Data.Split(',').ToList();
            Land = DataList[0];
            Breedtegraad = double.Parse(DataList[1], CultureInfo.InvariantCulture);
            Lengtegraad = double.Parse(DataList[2], CultureInfo.InvariantCulture);
        }
        string datumstring = DatumTijd();
        List<string> datumlist = datumstring.Split(" ").ToList();
        tijd = datumlist[0];
        datum = datumlist[1];
        string beschrijving = "";
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
        while (ongeldigeinvoer)
        {
            Console.WriteLine("Wilt u een beschrijving toevoegen? (ja/nee)");
            string tempinput = Console.ReadLine().ToLower();
            if (tempinput == "ja" || tempinput == "nee")
            {
                ongeldigeinvoer = false;
                if (tempinput == "ja")
                {
                    Console.WriteLine("Type de beschrijving");
                    beschrijving = Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("ongeldige invoer");
            }
        }
        ongeldigeinvoer = true;

        if (type == "dier")
        {
            Console.Write("Wat is het leefgebied? ");
            string Leefgebied = Console.ReadLine();

            var dier = new Dier(Naam, Oorsprong, Leefgebied, Land, Breedtegraad, Lengtegraad, beschrijving, tijd, datum);
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
                    var plant = new Plant(Naam, Oorsprong, hoogte, Land, Breedtegraad, Lengtegraad, beschrijving, tijd, datum);
                    ongeldigeinvoer = false;
                    _databaseService.VoegOrganismeToe(plant);
                }
                else { Console.WriteLine("Voer astublieft een geldig getal in."); }
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
                if (oorsprong == "exoot") { oorsprong = "exotisch"; }
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

    private static void VolledigeBeschrijving()
    {
        bool ongeldigeinvoer = true;
        var organismen = _databaseService.HaalAlleOrganismenOp();
        for (int i = 0; i < organismen.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {organismen[i].Naam}");
        }
        while (ongeldigeinvoer)
        {
            Console.WriteLine("Voer het nummer in van het dier dat je wilt zien");
            string num = Console.ReadLine();
            try
            {
                int numint = int.Parse(num);
                if (numint < organismen.Count + 1)
                {
                    Console.WriteLine(organismen[numint - 1].VoledigeBeschrijving());
                    ongeldigeinvoer = false;
                }
                else { Console.WriteLine($"Voer astublieft een geldig getal in 1 tot {organismen.Count}"); }
            }
            catch
            {
                Console.WriteLine("ongeldige invoer voer astublieft een getal in");
            }
        }
        Console.ReadKey();
    }

    private static string DatumTijd()
    {
        return DateTime.Now.ToString();
    }

    private static void SortAlpahbetisch()
    {
        var gesorteerd = _databaseService.FilterenOpAlfabetische();
        foreach (var organisme in gesorteerd)
        {
            Console.WriteLine(organisme.Beschrijving());
        }
        Console.WriteLine("\nDruk op een toets om door te gaan.");
        Console.ReadKey();
    }

}