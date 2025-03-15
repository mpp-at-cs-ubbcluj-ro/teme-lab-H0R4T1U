namespace ProiectMpp.Domain;

public class Player : Entity<int>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public int Team { get; set; }

    public Player(string name, string code, int team)
    {
        Name = name;
        Code = code;
        Team = team;
    }
    
}