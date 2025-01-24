public class Organisme
{
    public string Naam { get; set; }
    public string Type { get; set; }
    public string Oorsprong { get; set; }
    public string Land { get; set; }
    public double Breedtegraad { get; set; }
    public double Lengtegraad { get; set; }
    public string beschrijving { get; set; }
    public string tijd { get; set; }
    public string datum { get; set; }

    public Organisme(string type, string naam, string oorsprong, string land, double breedtegraad, double lengtegraad, string beschrijving, string tijd, string datum)
    {
        this.Type = type;
        this.Naam = naam;
        this.Oorsprong = oorsprong;
        this.Land = land;
        this.Breedtegraad = breedtegraad;
        this.Lengtegraad = lengtegraad;
        this.beschrijving = beschrijving;
        this.tijd = tijd;
        this.datum = datum;
    }

    public virtual string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()}.\n Gevonden in {Land} met de coördinaten {Breedtegraad}, {Lengtegraad}. \nOp {datum}, {tijd}";
    }
    public string VoledigeBeschrijving()
    {
        return beschrijving;
    }
}