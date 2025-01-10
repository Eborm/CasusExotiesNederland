public class Plant : Organisme
{
    public double HoogteInMeters { get; set; }

    public Plant(string naam, string oorsprong, double hoogte) : base ("plant",  naam, oorsprong)
    {
        this.HoogteInMeters = hoogte;
    }

    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en kan {HoogteInMeters} meter hoog worden.";
    }
}