using System.Data;
using log4net;
using log4net.Util;
using ProiectMpp.Domain;
using ProiectMpp.ConnectionUtills;

namespace ProiectMpp.Repository.DatabaseRepository;

public class PlayerDbRepository : IPlayerRepository
{

    private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDbRepository));
    private readonly IDbConnection _connection;
    public PlayerDbRepository(string connectionString)
    {
        _connection = ConnectionFactory.getInstance().createConnection(connectionString);
        Log.Info("Initializing PlayerDBRepository");
    }
    
    public Player? FindOne(int id)
    {
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Player WHERE Id = @id";
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            cmd.Parameters.Add(parameter);
            using IDataReader reader = cmd.ExecuteReader();
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
            _connection.Close();

        }
    }

    public IDictionary<int,Player> FindAll()
    {
        Log.Info($"Finding all players");
        Dictionary<int,Player> data = new Dictionary<int,Player>();
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Player";
            using IDataReader reader = cmd.ExecuteReader();
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
            _connection.Close();
        }
    }
    
    public Player Save(Player entity)
    {
        Log.Info("Saving player " + entity);
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Player (Name, Code,Team) VALUES (@name,@code, @team)";
            var name = cmd.CreateParameter();
            var code = cmd.CreateParameter();
            var team = cmd.CreateParameter();
            name.ParameterName = "@name";
            name.Value = entity.Name;
            code.ParameterName = "@code";
            code.Value = entity.Code;
            team.ParameterName = "@team";
            team.Value = entity.Team;
            
            cmd.Parameters.Add(name);
            cmd.Parameters.Add(code);
            cmd.Parameters.Add(team);
            
            cmd.ExecuteNonQuery();
            using (var command = _connection.CreateCommand())
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
            _connection.Close();
        }
    }

    public Player? Delete(int id)
    {
        Log.Info($"Deleting player {id}");
        Player? player = FindOne(id);
        if (player != null)
        {
            try
            {
                _connection.Open();
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Player WHERE id = @id";
                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;
                cmd.Parameters.Add(parameter);
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
                _connection.Close();
            }
        }
        Log.InfoExt("Player doesnt exist!");
        return null;

        
    }

    public Player Update(Player entity)
    {
        Log.Info("Updating player " + entity);
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Player SET Name = @name, Code = @code, Team = @team WHERE id = @id";
            
            var name = cmd.CreateParameter();
            var code = cmd.CreateParameter();
            var team = cmd.CreateParameter();
            var id = cmd.CreateParameter();
            id.ParameterName = "@id";
            id.Value = entity.Id;
            name.ParameterName = "@name";
            name.Value = entity.Name;
            code.ParameterName = "@code";
            code.Value = entity.Code;
            team.ParameterName = "@team";
            team.Value = entity.Team;
            
            cmd.Parameters.Add(name);
            cmd.Parameters.Add(id);
            cmd.Parameters.Add(code);
            cmd.Parameters.Add(team);
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
            _connection.Close();
        }
    }
}