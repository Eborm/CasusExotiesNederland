public class Dier : Organisme
{
    public string Leefgebied { get; set; }

    public Dier(string naam, string oorpsrong, string leefgebied, string land, double breedtegraad, double lengtegraad, string beschrijving) : base("dier", naam, oorpsrong, land, breedtegraad, lengtegraad, beschrijving)
    {
        this.Leefgebied = leefgebied;
    }


    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en leeft in {Leefgebied}. Gevonden in {Land} met de coördinaten {Breedtegraad}, {Lengtegraad}";
    }
}