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