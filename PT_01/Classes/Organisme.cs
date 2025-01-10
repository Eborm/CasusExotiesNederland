public class Organisme
{
    public string Naam { get; set; }
    public string Type { get; set; }
    public string Oorsprong { get; set; }

    public Organisme(string type, string naam, string oorsprong)
    {
        this.Type = type;
        this.Naam = naam;
        this.Oorsprong = oorsprong;
    }

    public virtual string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()}.";
    }
}