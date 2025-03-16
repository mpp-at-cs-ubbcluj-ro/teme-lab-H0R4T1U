using log4net.Util;
using ProiectMpp.Domain;
using Mono.Data.Sqlite;
namespace ProiectMpp.Repository;

public class PlayerDBRepository : AbstractDatabaseRepository<int, Player>
{


    public PlayerDBRepository(string connectionString) : base(connectionString)
    {
        Load();
        Log.Info("Initializing PlayerDBRepository");
    }

    public override Player? FindOne(int id)
    {
        Log.Info($"Finding player {id}");
        if (Data.ContainsKey(id))
        {
            Log.Info("Player MEM_HIT");
            return Data[id];
        }
        SqliteConnection connection = new SqliteConnection(ConnectionString);
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
                Data.Add(p.Id, p);
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

    public new IDictionary<int,Player> FindAll()
    {
        Log.Info($"Finding all players");
        return base.FindAll();
    }

    protected override void Load()
    {
        SqliteConnection connection = new SqliteConnection(ConnectionString);
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
                Data.Add(p.Id, p);
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
            throw new Exception(e.Message);
        }
        finally
        {
            Log.InfoExt("Found " + Data.Count + " players");
            connection.Close();
        }

    }
    public override Player Save(Player entity)
    {
        Log.Info("Saving player " + entity);
        SqliteConnection connection = new SqliteConnection(ConnectionString);
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

    public override Player Delete(int id)
    {
        Log.Info($"Deleting player {id}");
        SqliteConnection connection = new SqliteConnection(ConnectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Player WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            Player p = Data[id];
            Data.Remove(id);
            Log.InfoExt("Player deleted: " + id.ToString());
            return p;
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

    public override Player Update(Player entity)
    {
        Log.Info("Updating player " + entity);
        SqliteConnection connection = new SqliteConnection(ConnectionString);
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
            Player p = Data[entity.Id];
            Data[entity.Id] = entity;
            return p;
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