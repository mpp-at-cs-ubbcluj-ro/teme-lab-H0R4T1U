using System.Data;
using log4net;
using log4net.Util;
using ProiectMpp.Domain;
using ProiectMpp.ConnectionUtills;

namespace ProiectMpp.Repository.DatabaseRepository;

public class UserDbRepository : IUserRepository
{

    private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDbRepository));
    private readonly IDbConnection _connection;
    public UserDbRepository(string connectionString)
    {
        _connection = ConnectionFactory.getInstance().createConnection(connectionString);
        Log.Info("Initializing PlayerDBRepository");
    }
    
    public User? FindOne(int id)
    {
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM User WHERE Id = @id";
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            cmd.Parameters.Add(parameter);
            using IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                User p = new User(reader.GetString(1), reader.GetString(2));
                p.Id = reader.GetInt32(0);
                Log.InfoExt("User found: " + p);
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

    public IDictionary<int,User> FindAll()
    {
        Log.Info($"Finding all Users");
        Dictionary<int,User> data = new Dictionary<int,User>();
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM User";
            using IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                User p = new User(reader.GetString(1), reader.GetString(2));
                p.Id = reader.GetInt32(0);
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
            Log.InfoExt("Found " + data.Count + " Users");
            _connection.Close();
        }
    }
    
    public User Save(User entity)
    {
        Log.Info("Saving User " + entity);
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO User (Username, Password) VALUES (@username,@password)";
            var password = cmd.CreateParameter();
            var username = cmd.CreateParameter();
            username.ParameterName = "@username";
            password.ParameterName = "@password";
            username.Value = entity.Username;
            password.Value = entity.Password;
            
            cmd.Parameters.Add(username);
            cmd.Parameters.Add(password);
            cmd.ExecuteNonQuery();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT last_insert_rowid();";
                entity.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            Log.InfoExt("User saved: " + entity);
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

    public User? Delete(int id)
    {
        Log.Info($"Deleting User {id}");
        User? user = FindOne(id);
        if (user != null)
        {
            try
            {
                _connection.Open();
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "DELETE FROM User WHERE id = @id";
                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;
                cmd.Parameters.Add(id);
                cmd.ExecuteNonQuery();
                Log.InfoExt("User deleted: " + id.ToString());
                return user;
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
        Log.InfoExt("User doesnt exist!");
        return null;

        
    }

    public User Update(User entity)
    {
        Log.Info("Updating User " + entity);
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE User SET Username = @username, Password = @password WHERE id = @id";
            var id = cmd.CreateParameter();
            var username = cmd.CreateParameter();
            var password = cmd.CreateParameter();
            id.ParameterName = "@id";
            username.ParameterName = "@username";
            password.ParameterName = "@password";
            id.Value = entity.Id;
            username.Value = entity.Username;
            password.Value = entity.Password;
            
            cmd.Parameters.Add(id);
            cmd.Parameters.Add(username);
            cmd.Parameters.Add(password);
            cmd.ExecuteNonQuery();
            Log.InfoExt("User updated: " + entity);
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

    public User? FindByUsername(string username)
    {
        try
        {
            _connection.Open();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM User WHERE Username = @username";
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@username";
            parameter.Value = username; 
            cmd.Parameters.Add(parameter);
            using IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                User p = new User(reader.GetString(1), reader.GetString(2));
                p.Id = reader.GetInt32(0);
                Log.InfoExt("User found: " + p);
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

}