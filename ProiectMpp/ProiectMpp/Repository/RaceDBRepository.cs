using ProiectMpp.Domain;

using Mono.Data.Sqlite;

namespace ProiectMpp.Repository
{
    public class RaceDBRepository : AbstractDatabaseRepository<int, Race>
    {
        private readonly PlayerDBRepository _playerRepo;

        public RaceDBRepository(string connectionString,PlayerDBRepository repo) : base(connectionString)
        {
            _playerRepo = repo;
            Load();
            Log.Info($"Initialized RaceDBRepository with connection string: {ConnectionString}");
        }

        public override Race? FindOne(int id)
        {
            Log.Info($"Finding race with id {id}");
            if (Data.ContainsKey(id))
            {
                Log.Info("Race MEM_HIT");
                return Data[id];
            }
            using var connection = new SqliteConnection(ConnectionString);
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
                    Data.Add(id, race);
                    return race;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            return null;
        }

        public new IDictionary<int, Race> FindAll()
        {
            Log.Info("Finding all Races");
            return base.FindAll();
        }

        protected override void Load()
        {
            Log.Info("Finding all races");
            using var connection = new SqliteConnection(ConnectionString);
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
                    Data.Add(race.Id, race);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new Exception(e.Message);
            }
        }

        public override Race Save(Race entity)
        {
            Log.Info("Saving Race " + entity);
            using var connection = new SqliteConnection(ConnectionString);
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
                Data.Add(entity.Id, entity);
                return entity;
            }
            catch (Exception e)
            {
                Log.Error(e);
                transaction.Rollback();
                throw new Exception(e.Message);
            }
        }

        public override Race? Delete(int id)
        {
            Log.Info($"Deleting race with id {id}");
            Race? race = FindOne(id);
            if (race == null) return null;

            using var connection = new SqliteConnection(ConnectionString);
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
                Data.Remove(id);
                return race;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Log.Error(e);
                return null;
            }
        }
        public override Race Update(Race entity)
        {
            Log.Info($"Updating race {entity}");
            using var connection = new SqliteConnection(ConnectionString);
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
                Race r = Data[entity.Id];
                Data[entity.Id] = entity;
                r.NoPlayers = entity.Players.Count;
                return r;
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