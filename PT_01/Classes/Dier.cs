public class Dier : Organisme
{
    public string Leefgebied { get; set; }

    public Dier(string naam, string oorpsrong, string leefgebied) : base("dier", naam, oorpsrong)
    {
        this.Leefgebied = leefgebied;
    }


    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en leeft in {Leefgebied}.";
    }
}