using System.Data;
using log4net;
using ProiectMpp.Domain;
using log4net.Util;
using ProiectMpp.ConnectionUtills;

namespace ProiectMpp.Repository.DatabaseRepository;

public class TeamDbRepository: ITeamRepository
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TeamDbRepository));
    private readonly IDbConnection _connection;
    public TeamDbRepository(string connectionString)
    {
        _connection = ConnectionFactory.getInstance().createConnection(connectionString);
        Log.Info("Initializing PlayerDBRepository ");
    }

     
    public Team? FindOne(int id)
    {
        Log.Info($"Finding team with id {id}");

        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Team WHERE id = @id";
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            cmd.Parameters.Add(parameter);
            using IDataReader reader = cmd.ExecuteReader();
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
                _connection.Close();
        }
        
    }
     
    public IDictionary<int,Team> FindAll()
    {
        Log.Info("Retrieving all teams");
        Dictionary<int,Team> data = new Dictionary<int,Team>();
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Team";
            using IDataReader reader = cmd.ExecuteReader();
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
            _connection.Close();
        }
        
    }
    
    
    public Team Save(Team entity)
    {
        Log.Info($"Saving team with id {entity.Id}");
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Team (Name) VALUES (@name)";
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@name";
            parameter.Value = entity.Name;
            cmd.Parameters.Add(parameter);
            cmd.ExecuteNonQuery();
            using (var command = _connection.CreateCommand())
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
            _connection.Close();
        }
    }

    public Team? Delete(int id)
    {
        Log.Info($"Deleting team with id {id}");
        Team? team = FindOne(id);
        if (team != null)
        {
            try
            {
                _connection.Open();
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Team WHERE id = @id";
                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;
                cmd.Parameters.Add(parameter);
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
                _connection.Close();
            }
        }

        return null;
    }

    public Team Update(Team entity)
    {
        Log.Info($"Updating team with id {entity.Id}");
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Team SET Name = @name WHERE id = @id";
            var name = cmd.CreateParameter();
            name.ParameterName = "@name";
            name.Value = entity.Name;
            
            var id = cmd.CreateParameter();
            id.ParameterName = "@id";
            id.Value = entity.Id;
            
            cmd.Parameters.Add(name);
            cmd.Parameters.Add(id);
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
            _connection.Close();
        }
    }
}