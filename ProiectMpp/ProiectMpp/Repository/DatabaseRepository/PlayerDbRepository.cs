using log4net;
using log4net.Util;
using ProiectMpp.Domain;
using Mono.Data.Sqlite;
namespace ProiectMpp.Repository.DatabaseRepository;

public class PlayerDbRepository : IPlayerRepository
{

    private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDbRepository));
    private readonly string _connectionString;
    public PlayerDbRepository(string connectionString)
    {
        _connectionString = connectionString;
        Log.Info("Initializing PlayerDBRepository");
    }
    
    public Player? FindOne(int id)
    {
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Player WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using SqliteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Player p = new Player(reader.GetString(0), reader.GetString(2), reader.GetInt32(3));
                p.Id = reader.GetInt32(1);
                Log.InfoExt("Player found: " + p);
                return p;
            }
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e);
            throw new Exception(e.Message);
        }
        finally
        {
            connection.Close();

        }
    }

    public IDictionary<int,Player> FindAll()
    {
        Log.Info($"Finding all players");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        Dictionary<int,Player> data = new Dictionary<int,Player>();
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Player";
            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Player p = new Player(reader.GetString(0), reader.GetString(2), reader.GetInt32(3));
                p.Id = reader.GetInt32(1);
                data.Add(p.Id, p);
            }
            return data;
        }
        catch (Exception e)
        {
            Log.Error(e);
            throw new Exception(e.Message);
        }
        finally
        {
            Log.InfoExt("Found " + data.Count + " players");
            connection.Close();
        }
    }
    
    public Player Save(Player entity)
    {
        Log.Info("Saving player " + entity);
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Player (Name, Code,Team) VALUES (@name,@code, @team)";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@code", entity.Code);
            cmd.Parameters.AddWithValue("@team", entity.Team);
            cmd.ExecuteNonQuery();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid();";
                entity.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            Log.InfoExt("Player saved: " + entity);
            return entity;
        }
        catch (Exception e)
        {
            Log.Error(e);
            throw new Exception(e.Message);
        }
        finally
        {
            connection.Close();
        }
    }

    public Player? Delete(int id)
    {
        Log.Info($"Deleting player {id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        Player? player = FindOne(id);
        if (player != null)
        {
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Player WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                Log.InfoExt("Player deleted: " + id.ToString());
                return player;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new Exception(e.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        Log.InfoExt("Player doesnt exist!");
        return null;

        
    }

    public Player Update(Player entity)
    {
        Log.Info("Updating player " + entity);
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Player SET Name = @name, Code = @code, Team = @team WHERE id = @id";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@code", entity.Code);
            cmd.Parameters.AddWithValue("@team", entity.Team);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Player updated: " + entity);
            return entity;
        }
        catch (Exception e)
        {
            Log.Error(e);
            throw new Exception(e.Message);
        }
        finally
        {
            connection.Close();
        }
    }
}