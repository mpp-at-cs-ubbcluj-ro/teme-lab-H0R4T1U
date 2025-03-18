using log4net;
using log4net.Util;
using ProiectMpp.Domain;
using Mono.Data.Sqlite;
namespace ProiectMpp.Repository.DatabaseRepository;

public class UserDbRepository : IUserRepository
{

    private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerDbRepository));
    private readonly string _connectionString;
    public UserDbRepository(string connectionString)
    {
        _connectionString = connectionString;
        Log.Info("Initializing PlayerDBRepository");
    }
    
    public User? FindOne(int id)
    {
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM User WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using SqliteDataReader reader = cmd.ExecuteReader();
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
            connection.Close();

        }
    }

    public IDictionary<int,User> FindAll()
    {
        Log.Info($"Finding all Users");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        Dictionary<int,User> data = new Dictionary<int,User>();
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM User";
            using SqliteDataReader reader = cmd.ExecuteReader();
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
            connection.Close();
        }
    }
    
    public User Save(User entity)
    {
        Log.Info("Saving User " + entity);
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO User (Username, Password) VALUES (@username,@password)";
            cmd.Parameters.AddWithValue("@username", entity.Username);
            cmd.Parameters.AddWithValue("@password", entity.Password);
            cmd.ExecuteNonQuery();
            using (var command = connection.CreateCommand())
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
            connection.Close();
        }
    }

    public User? Delete(int id)
    {
        Log.Info($"Deleting User {id}");
        SqliteConnection connection = new SqliteConnection(_connectionString);
        User? user = FindOne(id);
        if (user != null)
        {
            try
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM User WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
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
                connection.Close();
            }
        }
        Log.InfoExt("User doesnt exist!");
        return null;

        
    }

    public User Update(User entity)
    {
        Log.Info("Updating User " + entity);
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE User SET Username = @username, Password = @password WHERE id = @id";
            cmd.Parameters.AddWithValue("@username", entity.Username);
            cmd.Parameters.AddWithValue("@password", entity.Password);
            cmd.Parameters.AddWithValue("@id", entity.Id);
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
            connection.Close();
        }
    }

    public User? FindByUsername(string username)
    {
        SqliteConnection connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM User WHERE Username = @username";
            cmd.Parameters.AddWithValue("@username", username);
            using SqliteDataReader reader = cmd.ExecuteReader();
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
            connection.Close();

        }
    }

}