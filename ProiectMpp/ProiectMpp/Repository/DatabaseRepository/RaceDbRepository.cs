using log4net;
using ProiectMpp.Domain;

using Mono.Data.Sqlite;

namespace ProiectMpp.Repository.DatabaseRepository
{
    public class RaceDbRepository : IRaceRepository
    {
        private readonly PlayerDbRepository _playerRepo;
        private static readonly ILog Log = LogManager.GetLogger(typeof(RaceDbRepository));
        private readonly string _connectionString;

        public RaceDbRepository(string connectionString, PlayerDbRepository playerRepo)
        {
            _playerRepo = playerRepo;
            _connectionString = connectionString;
            Log.Info("Initializing PlayerDBRepository");
        }

        public Race? FindOne(int id)
        {
            Log.Info($"Finding race with id {id}");
    
            using var connection = new SqliteConnection(_connectionString);
            Race? race = null;
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Race WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int engineType = reader.GetInt32(reader.GetOrdinal("EngineType"));
                    int noPlayers = reader.GetInt32(reader.GetOrdinal("NoPlayers"));
                    race = new Race(engineType) { Id = id, NoPlayers = noPlayers };
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new Exception(e.Message);
            }

            if (race != null)
            {
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT PlayerId FROM PlayerRaces WHERE RaceId = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int playerId = reader.GetInt32(reader.GetOrdinal("PlayerId"));
                        Player? player = _playerRepo.FindOne(playerId);
                        if (player != null)
                        {
                            race.Players.Add(player);
                        }
                    }
                    race.NoPlayers = race.Players.Count;
                    return race;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            return null;
        }

        public IDictionary<int, Race> FindAll()
        {
            Log.Info("Finding all Races");
            using var connection = new SqliteConnection(_connectionString);
            Dictionary<int, Race> data = new Dictionary<int, Race>();
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Race";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("Id"));
                    int engineType = reader.GetInt32(reader.GetOrdinal("EngineType"));
                    int noPlayers = reader.GetInt32(reader.GetOrdinal("NoPlayers"));
                    var race = new Race(engineType) { Id = id, NoPlayers = noPlayers };

                    using var cmdPlayers = connection.CreateCommand();
                    cmdPlayers.CommandText = "SELECT PlayerId FROM PlayerRaces WHERE RaceId = @id";
                    cmdPlayers.Parameters.AddWithValue("@id", id);
                    using var readerPlayers = cmdPlayers.ExecuteReader();
                    while (readerPlayers.Read())
                    {
                        int playerId = readerPlayers.GetInt32(readerPlayers.GetOrdinal("PlayerId"));
                        Player? player = _playerRepo.FindOne(playerId);
                        if (player != null)
                        {
                            race.Players.Add(player);
                        }
                    }
                    race.NoPlayers = race.Players.Count;
                    data.Add(race.Id, race);
                }
                Log.Info("Found " + data.Count + " Races");
                return data;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new Exception(e.Message);
            }
        }


        public  Race Save(Race entity)
        {
            Log.Info("Saving Race " + entity);
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO Race (EngineType, NoPlayers) VALUES (@engineType, @noPlayers)";
                cmd.Parameters.AddWithValue("@engineType", entity.EngineType);
                cmd.Parameters.AddWithValue("@noPlayers", entity.Players.Count);
                cmd.ExecuteNonQuery();

                using var cmdLastId = connection.CreateCommand();
                cmdLastId.Transaction = transaction;
                cmdLastId.CommandText = "SELECT last_insert_rowid();";
                entity.Id = Convert.ToInt32(cmdLastId.ExecuteScalar());

                foreach (var player in entity.Players)
                {
                    using var cmdPlayers = connection.CreateCommand();
                    cmdPlayers.Transaction = transaction;
                    cmdPlayers.CommandText = "INSERT OR IGNORE INTO PlayerRaces (PlayerId, RaceId) VALUES (@playerId, @raceId)";
                    cmdPlayers.Parameters.AddWithValue("@playerId", player.Id);
                    cmdPlayers.Parameters.AddWithValue("@raceId", entity.Id);
                    cmdPlayers.ExecuteNonQuery();
                }

                entity.NoPlayers = entity.Players.Count;
                transaction.Commit();
                return entity;
            }
            catch (Exception e)
            {
                Log.Error(e);
                transaction.Rollback();
                throw new Exception(e.Message);
            }
        }

        public Race? Delete(int id)
        {
            Log.Info($"Deleting race with id {id}");
            Race? race = FindOne(id);
            if (race == null) return null;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmdPlayers = connection.CreateCommand();
                cmdPlayers.Transaction = transaction;
                cmdPlayers.CommandText = "DELETE FROM PlayerRaces WHERE RaceId = @id";
                cmdPlayers.Parameters.AddWithValue("@id", id);
                cmdPlayers.ExecuteNonQuery();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM Race WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                
                transaction.Commit();
                return race;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Log.Error(e);
                return null;
            }
        }
        public Race Update(Race entity)
        {
            Log.Info($"Updating race {entity}");
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = "UPDATE Race SET EngineType = @engineType, NoPlayers = @noPlayers WHERE Id = @id";
                cmd.Parameters.AddWithValue("@engineType", entity.EngineType);
                cmd.Parameters.AddWithValue("@noPlayers", entity.Players.Count);
                cmd.Parameters.AddWithValue("@id", entity.Id);
                cmd.ExecuteNonQuery();

                using var cmdDeletePlayers = connection.CreateCommand();
                cmdDeletePlayers.Transaction = transaction;
                cmdDeletePlayers.CommandText = "DELETE FROM PlayerRaces WHERE RaceId = @id";
                cmdDeletePlayers.Parameters.AddWithValue("@id", entity.Id);
                cmdDeletePlayers.ExecuteNonQuery();

                foreach (var player in entity.Players)
                {
                    using var cmdPlayers = connection.CreateCommand();
                    cmdPlayers.Transaction = transaction;
                    cmdPlayers.CommandText = "INSERT OR IGNORE INTO PlayerRaces (PlayerId, RaceId) VALUES (@playerId, @raceId)";
                    cmdPlayers.Parameters.AddWithValue("@playerId", player.Id);
                    cmdPlayers.Parameters.AddWithValue("@raceId", entity.Id);
                    cmdPlayers.ExecuteNonQuery();
                }

                transaction.Commit();
                return entity;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Log.Error(e);
                throw new Exception(e.Message);
            }
        }
    }
}