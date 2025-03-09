namespace ProiectMpp.Domain;

public class Player : Entity<long>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Team { get; set; }

    public Player(string name, string code, string team)
    {
        Name = name;
        Code = code;
        Team = team;
    }
}