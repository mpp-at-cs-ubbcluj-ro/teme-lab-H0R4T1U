using System.Data;
using Mono.Data.Sqlite;

namespace ProiectMpp.ConnectionUtills
{
	public class SqliteConnectionFactory : ConnectionFactory
	{
		public override IDbConnection createConnection(string connectionString)
		{
			//Mono Sqlite Connection

			Console.WriteLine("SQLite ---Se deschide o conexiune la  ... {0}", connectionString);
			return new SqliteConnection(connectionString);

			// Windows SQLite Connection, fisierul .db ar trebuie sa fie in directorul bin/debug
			//String connectionString = "Data Source=tasks.db;Version=3";
			//return new SQLiteConnection(connectionString);
		}
	}
}
