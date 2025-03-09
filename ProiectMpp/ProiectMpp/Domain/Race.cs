namespace ProiectMpp.Domain;

public class Race : Entity<long>
{
    public int EngineType { get; set; }
    public int NoPlayers { get; set; }

    public List<Player> Players
    {
        get => Players;
        set
        {
            Players = new List<Player>();
            Players.AddRange(value);
            NoPlayers = Players.Count;
            
        }
    }

    public Race(int engineType)
    {
        EngineType = engineType;
        NoPlayers = 0;
        Players = new List<Player>();
    }
    
}