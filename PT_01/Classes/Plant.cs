public class Plant : Organisme
{
    public double HoogteInMeters { get; set; }

    public Plant()
    {
        Type = "Plant";
    }

    public override string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()} en kan {HoogteInMeters} meter hoog worden.";
    }
}