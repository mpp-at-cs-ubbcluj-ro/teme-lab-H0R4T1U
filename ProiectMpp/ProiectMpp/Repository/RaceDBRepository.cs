
using ProiectMpp.Domain;
using log4net;
using Mono.Data.Sqlite;

namespace ProiectMpp.Repository
{
    public class RaceDBRepository : IRepository<int, Race>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RaceDBRepository));
        private readonly string _connectionString;

        public RaceDBRepository(string connectionString)
        {
            _connectionString = connectionString;
            Log.Info($"Initialized RaceDBRepository with connection string: {_connectionString}");
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
            }

            if (race != null)
            {
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT PlayerId FROM PlayerRaces WHERE RaceId = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    var playerIds = new List<int>();
                    while (reader.Read())
                    {
                        playerIds.Add(reader.GetInt32(reader.GetOrdinal("PlayerId")));
                    }
                    race.PlayerIds = playerIds;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            return race;
        }

        public IDictionary<int,Race> FindAll()
        {
            Log.Info("Finding all races");
            Dictionary<int,Race> races = new Dictionary<int,Race>() ;
            using var connection = new SqliteConnection(_connectionString);
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
                    var playerIds = new List<int>();
                    while (readerPlayers.Read())
                    {
                        playerIds.Add(readerPlayers.GetInt32(readerPlayers.GetOrdinal("PlayerId")));
                    }
                    race.PlayerIds = playerIds;

                    races.Add(race.Id,race);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return races;
        }

        public Race? Save(Race entity)
        {
            Log.Info($"Saving race {entity}");
            using var connection = new SqliteConnection(_connectionString);
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Race (EngineType, NoPlayers) VALUES (@engineType, @noPlayers)";
                cmd.Parameters.AddWithValue("@engineType", entity.EngineType);
                cmd.Parameters.AddWithValue("@noPlayers", entity.NoPlayers);
                cmd.ExecuteNonQuery();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT last_insert_rowid();";
                    entity.Id = Convert.ToInt32(command.ExecuteScalar());
                }

                foreach (var playerId in entity.PlayerIds)
                {
                    using var cmdPlayers = connection.CreateCommand();
                    cmdPlayers.CommandText = "INSERT INTO PlayerRaces (PlayerId, RaceId) VALUES (@playerId, @raceId)";
                    cmdPlayers.Parameters.AddWithValue("@playerId", playerId);
                    cmdPlayers.Parameters.AddWithValue("@raceId", entity.Id);
                    cmdPlayers.ExecuteNonQuery();
                }
                Log.Info($"Saved {entity}");
                return entity;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public Race? Delete(int id)
        {
            Log.Info($"Deleting race with id {id}");
            Race? race = FindOne(id);
            if (race == null) return null;

            using var connection = new SqliteConnection(_connectionString);
            try
            {
                connection.Open();
                using var cmdPlayers = connection.CreateCommand();
                cmdPlayers.CommandText = "DELETE FROM PlayerRaces WHERE RaceId = @id";
                cmdPlayers.Parameters.AddWithValue("@id", id);
                cmdPlayers.ExecuteNonQuery();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Race WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return race;
        }

        public Race? Update(Race entity)
        {
            Log.Info($"Updating race {entity}");
            using var connection = new SqliteConnection(_connectionString);
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "UPDATE Race SET EngineType = @engineType, NoPlayers = @noPlayers WHERE Id = @id";
                cmd.Parameters.AddWithValue("@engineType", entity.EngineType);
                cmd.Parameters.AddWithValue("@noPlayers", entity.NoPlayers);
                cmd.Parameters.AddWithValue("@id", entity.Id);
                cmd.ExecuteNonQuery();

                using var cmdDeletePlayers = connection.CreateCommand();
                cmdDeletePlayers.CommandText = "DELETE FROM PlayerRaces WHERE RaceId = @id";
                cmdDeletePlayers.Parameters.AddWithValue("@id", entity.Id);
                cmdDeletePlayers.ExecuteNonQuery();

                foreach (var playerId in entity.PlayerIds)
                {
                    using var cmdPlayers = connection.CreateCommand();
                    cmdPlayers.CommandText = "INSERT INTO PlayerRaces (PlayerId, RaceId) VALUES (@playerId, @raceId)";
                    cmdPlayers.Parameters.AddWithValue("@playerId", playerId);
                    cmdPlayers.Parameters.AddWithValue("@raceId", entity.Id);
                    cmdPlayers.ExecuteNonQuery();
                }
                return entity;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }
    }
}