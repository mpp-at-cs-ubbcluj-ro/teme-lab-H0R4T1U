namespace ProiectMpp.Domain;

public class User:Entity<int>
{
    public String Username { get; set; }
    public String Password { get; set; }

    public User(string username, string password)
    {
        Username = username;
        Password = password;
    }
    
}