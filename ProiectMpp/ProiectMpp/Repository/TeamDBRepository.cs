using log4net;
using ProiectMpp.Domain;
using log4net.Util;
using Mono.Data.Sqlite;

namespace ProiectMpp.Repository;

public class TeamDBRepository: IRepository<int,Team>
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDBRepository));
    private readonly string _connectionString;

    public TeamDBRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    public Team? FindOne(int id)
    {
        Log.Info($"Finding team with id {id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Team WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using SqliteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Team t = new Team(reader.GetString(0));
                t.Id = reader.GetInt32(1);
                Log.InfoExt("Team found: " + t);
                return t;
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

    public IDictionary<int,Team> FindAll()
    {
        Log.Info($"Finding teams");
        Dictionary<int,Team> teams = new Dictionary<int, Team>();
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Teams";
            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Team t = new Team(reader.GetString(0));
                t.Id = reader.GetInt32(1);
                teams.Add(t.Id, t);
            }
            Log.InfoExt("Teams found: " + teams.Count.ToString());
            return teams;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return teams;
        }
        finally
        {
            connection.Close();
        }
        
    }

    public Team? Save(Team entity)
    {
        Log.Info($"Saving team with id {entity.Id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Team (Name,Id) VALUES (@name,@id)";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Team saved: " + entity);
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

    public Team? Delete(int id)
    {
        Log.Info($"Deleting team with id {id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Team WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Team deleted: " + id.ToString());
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e);
            Team t = new Team("");
            t.Id = id;
            return t ;
        }
        finally
        {
            connection.Close();
        }
    }

    public Team? Update(Team entity)
    {
        Log.Info($"Updating team with id {entity.Id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Team SET Name = @name WHERE id = @id";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Team updated: " + entity);
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