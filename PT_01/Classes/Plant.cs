public class Plant : Organisme
{
    public double HoogteInMeters { get; set; }

    public Plant(string naam, string oorsprong, double hoogte, string land, double breedtegraad, double lengtegraad, string beschrijving, string tijd, string datum) : base("plant", naam, oorsprong, land, breedtegraad, lengtegraad, beschrijving, tijd, datum)
    {
        HoogteInMeters = hoogte;
    }

    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en kan {HoogteInMeters} meter hoog worden. \n Gevonden in {Land} met de coördinaten {Breedtegraad}, {Lengtegraad}. \nOp {datum}, {tijd}";
    }
}