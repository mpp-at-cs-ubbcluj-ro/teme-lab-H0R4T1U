using ProiectMpp.Domain;
using log4net.Util;
using Mono.Data.Sqlite;

namespace ProiectMpp.Repository;

public class TeamDBRepository: AbstractDatabaseRepository<int,Team>
{
    public TeamDBRepository(string connectionString) : base(connectionString)
    {
        Load();
        Log.Info("Initializing TeamDBRepository");
    }
     
    public override Team? FindOne(int id)
    {
        Log.Info($"Finding team with id {id}");
        if (Data.ContainsKey(id))
        {
            Log.Info("Team MEM_HIT");
            return Data[id];
        }
        
        SqliteConnection connection = new SqliteConnection(ConnectionString);
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
                Data.Add(t.Id, t);
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
     
    public new IDictionary<int,Team> FindAll()
    {
        Log.Info("Retrieving all teams");
        return base.FindAll();
        
    }

    protected override void Load()
    {
        Log.Info($"Finding teams");

        SqliteConnection connection = new SqliteConnection(ConnectionString);
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
                Data.Add(t.Id, t);
            }
            Log.InfoExt("Teams found: " + Data.Count.ToString());

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
    
    public override Team Save(Team entity)
    {
        Log.Info($"Saving team with id {entity.Id}");
        SqliteConnection connection = new SqliteConnection(ConnectionString);
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
            Data.Add(entity.Id, entity);
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

    public override Team Delete(int id)
    {
        Log.Info($"Deleting team with id {id}");
        SqliteConnection connection = new SqliteConnection(ConnectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Team WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Team deleted: " + id.ToString());
            Team t = Data[id];
            Data.Remove(id);
            return t;
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

    public override Team Update(Team entity)
    {
        Log.Info($"Updating team with id {entity.Id}");
        SqliteConnection connection = new SqliteConnection(ConnectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Team SET Name = @name WHERE id = @id";
            cmd.Parameters.AddWithValue("@name", entity.Name);
            cmd.Parameters.AddWithValue("@id", entity.Id);
            cmd.ExecuteNonQuery();
            Log.InfoExt("Team updated: " + entity);
            Team t = Data[entity.Id];
            Data[entity.Id] = entity;
            return t;
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