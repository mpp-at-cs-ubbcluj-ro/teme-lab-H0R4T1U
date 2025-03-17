using log4net;
using ProiectMpp.Domain;
using log4net.Util;
using Mono.Data.Sqlite;

namespace ProiectMpp.Repository.DatabaseRepository;

public class TeamDbRepository: ITeamRepository
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TeamDbRepository));
    private readonly string _connectionString;
    public TeamDbRepository(string connectionString)
    {
        _connectionString = connectionString;
        Log.Info("Initializing PlayerDBRepository ");
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
                throw new Exception(e.Message);
        }
        finally
        {
                connection.Close();
        }
        
    }
     
    public IDictionary<int,Team> FindAll()
    {
        Log.Info("Retrieving all teams");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        Dictionary<int,Team> data = new Dictionary<int,Team>();
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Team";
            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Team t = new Team(reader.GetString(0));
                t.Id = reader.GetInt32(1);
                data.Add(t.Id, t);
            }
            Log.InfoExt("Teams found: " + data.Count.ToString());
            return data;

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
    
    
    public Team Save(Team entity)
    {
        Log.Info($"Saving team with id {entity.Id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Team (Name) VALUES (@name)";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.ExecuteNonQuery();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid();";
                entity.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            Log.InfoExt("Team saved: " + entity);
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

    public Team? Delete(int id)
    {
        Log.Info($"Deleting team with id {id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        Team? team = FindOne(id);
        if (team != null)
        {
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Team WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                Log.InfoExt("Team deleted: " + id.ToString());
                return team;
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

        return null;
    }

    public Team Update(Team entity)
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