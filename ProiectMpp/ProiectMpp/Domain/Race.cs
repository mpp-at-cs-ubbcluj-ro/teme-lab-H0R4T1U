namespace ProiectMpp.Domain;

public class Race : Entity<int>
{
    public int EngineType { get; set; }
    public int NoPlayers { get; set; }

    public List<Player> Players { get; set; }
    public Race(int engineType)
    {
        EngineType = engineType;
        NoPlayers = 0;
        Players = new List<Player>();
    }
    
}