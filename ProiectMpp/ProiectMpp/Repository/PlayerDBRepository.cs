using log4net;
using log4net.Util;
using ProiectMpp.Domain;
using Mono.Data.Sqlite;
namespace ProiectMpp.Repository;

public class PlayerDBRepository : IRepository<int, Player>
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDBRepository));
    private readonly string _connectionString;

    public PlayerDBRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Player? FindOne(int id)
    {
        Log.Info($"Finding player {id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Player WHERE id = @id";
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
            return null;
        }
        finally
        {
            connection.Close();

        }
    }

    public IDictionary<int,Player> FindAll()
    {
        Log.Info($"Finding all players");
        Dictionary<int, Player> players = new Dictionary<int, Player>();
        SqliteConnection connection = new SqliteConnection(_connectionString);
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
                players.Add(p.Id, p);
            }
            return players;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return players;
        }
        finally
        {
            Log.InfoExt("Found " + players.Count.ToString() + " players");
            connection.Close();
        }
        
    }

    public Player? Save(Player entity)
    {
        Log.Info("Saving player " + entity);
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Player (Name,Id, Code,Team) VALUES (@name, @id,@code, @team)";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.Parameters.AddWithValue("@code", entity.Code);
            cmd.Parameters.AddWithValue("@team", entity.Team);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Player saved: " + entity);
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return entity;
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
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Player WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Player deleted: " + id.ToString());
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e);
            Player t = new Player("","",0);
            t.Id= id;
            return t;
        }
        finally
        {
            connection.Close();
        }
    }

    public Player? Update(Player entity)
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
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return entity;
        }
        finally
        {
            connection.Close();
        }
    }
}