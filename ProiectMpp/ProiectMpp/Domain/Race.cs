namespace ProiectMpp.Domain;

public class Race : Entity<int>
{
    public int EngineType { get; set; }
    public int NoPlayers { get; set; }

    public List<int> PlayerIds { get; set; }
    public Race(int engineType)
    {
        EngineType = engineType;
        NoPlayers = 0;
        PlayerIds = new List<int>();
    }
    
}