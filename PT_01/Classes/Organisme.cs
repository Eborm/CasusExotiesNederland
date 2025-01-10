public class Organisme
{
    public string Naam { get; set; }
    public string Type { get; set; }
    public string Oorsprong { get; set; }

    public virtual string Beschrijving()
    {
        return $"De {Naam} is een {Oorsprong.ToLower()} {Type.ToLower()}.";
    }
}