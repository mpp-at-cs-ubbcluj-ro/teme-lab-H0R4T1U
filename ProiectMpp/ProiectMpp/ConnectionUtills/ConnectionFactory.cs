using System.Data;
using System.Reflection;

namespace ProiectMpp.ConnectionUtills
{
	public abstract class ConnectionFactory
	{
		protected ConnectionFactory()
		{
		}

		private static ConnectionFactory _instance;

		public static ConnectionFactory getInstance()
		{
			if (_instance == null)
			{

				Assembly assem = Assembly.GetExecutingAssembly();
				Type[] types = assem.GetTypes();
				foreach (var type in types)
				{
					if (type.IsSubclassOf(typeof(ConnectionFactory)))
						_instance = (ConnectionFactory)Activator.CreateInstance(type);
				}
			}
			return _instance;
		}

		public abstract  IDbConnection createConnection(string props);
	}




}
