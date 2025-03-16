// See https://aka.ms/new-console-template for more information
using System.Configuration;
using log4net.Config;
using log4net;
using System.Reflection;
using Mono.Data.Sqlite;
using ProiectMpp.Domain;
using ProiectMpp.Repository;



namespace ProiectMpp;

internal class Program
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
    static void Main(string[] args)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            var logRepository = LogManager.GetRepository(entryAssembly);
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            Log.Info("Hello, World!");
        }
        
        string settings = ConfigurationManager.ConnectionStrings["MotoDB"].ConnectionString;
        PlayerDBRepository playerDbRepository = new PlayerDBRepository(settings);
        Console.WriteLine("All players in the database:");
        foreach (var kvp in playerDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }

        TeamDBRepository teamDbRepository = new TeamDBRepository(settings);
        Console.WriteLine("All teams in the database:");
        foreach (var kvp in teamDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }

        RaceDBRepository raceDbRepository = new RaceDBRepository(settings, playerDbRepository);
        Console.WriteLine("All races in the database:");
        foreach (var kvp in raceDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }

        Team team = new Team("McLaren");
        var teamOptional = teamDbRepository.Save(team);
        if (teamOptional != null)
        {
            Console.WriteLine(teamOptional);
        }
        else
        {
            Console.WriteLine("The team already exists in the database");
        }

        Player player = new Player("Norris", "1110001110001", 3);
        var playerOptional = playerDbRepository.Save(player);
        if (playerOptional != null)
        {
            Console.WriteLine(playerOptional);
        }
        else
        {
            Console.WriteLine("The player already exists in the database");
        }

        Race r = raceDbRepository.FindOne(1);
        if (r != null)
        {
            List<Player> players = r.Players;
            players.Add(player);
            r.Players = players;
            r.NoPlayers = players.Count;
            raceDbRepository.Update(r);
        }

        Console.WriteLine("All races in the database after update:");
        foreach (var kvp in raceDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }
        
    }
}

