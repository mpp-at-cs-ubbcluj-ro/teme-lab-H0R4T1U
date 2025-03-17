// See https://aka.ms/new-console-template for more information
using System.Configuration;
using log4net.Config;
using log4net;
using System.Reflection;
using log4net.Util;
using ProiectMpp.Domain;
using ProiectMpp.Repository.DatabaseRepository;


namespace ProiectMpp;

internal class Program
{
    static void Main(string[] args)
    {
        Team team = new Team("ECHIPA TEAM");
        Player player = new Player("James May", "1110001110001", 5);
       


        
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        
        string settings = ConfigurationManager.ConnectionStrings["MotoDB"].ConnectionString;
        PlayerDbRepository playerDbRepository = new PlayerDbRepository(settings);
        Console.WriteLine("All players in the database:");
        foreach (var kvp in playerDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }

        TeamDbRepository teamDbRepository = new TeamDbRepository(settings);
        Console.WriteLine("All teams in the database:");
        foreach (var kvp in teamDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }

        RaceDbRepository raceDbRepository = new RaceDbRepository(settings, playerDbRepository);
        Console.WriteLine("All races in the database:");
        foreach (var kvp in raceDbRepository.FindAll())
        {
            Console.WriteLine($"{kvp.Key} => {kvp.Value}");
        }

        var teamOptional = teamDbRepository.Save(team);
        if (teamOptional != null)
        {
            Console.WriteLine(teamOptional);
        }
        else
        {
            Console.WriteLine("The team already exists in the database");
        }

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

