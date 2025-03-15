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
        PlayerDBRepository playerRepository = new PlayerDBRepository(settings);
        TeamDBRepository teamRepository = new TeamDBRepository(settings);
        RaceDBRepository raceRepository = new RaceDBRepository(settings);
        Player? player = playerRepository.FindOne(1);
        Team? team = teamRepository.FindOne(1);
        Race? race = raceRepository.FindOne(1);
        Console.WriteLine(player);


    }
}

