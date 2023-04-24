using CloudStructures;
using SqlKata.Compilers;
using SqlKata.Execution;
using MySqlConnector;

namespace TuesberryAPIServer
{
    public class DBManager
    {
        static string DBConnectionString = string.Empty;
        static string RedisAddress = string.Empty;

        public static RedisConnection? RedisConn { get; set; }   

        public static void Init(IConfiguration configuration)
        {
            DBConnectionString = configuration.GetSection("DBConnection")["MySQLConnection"] ?? string.Empty;
            RedisAddress = configuration.GetSection("DBConnection")["Redis"] ?? string.Empty;

            var config = new RedisConfig("basic", RedisAddress);
            RedisConn = new RedisConnection(config);    
        }

        public static async Task<QueryFactory> GetDBQuery()
        {
            var connection = new MySqlConnection(DBConnectionString);   
            await connection.OpenAsync();

            var compiler = new MySqlCompiler();
            var queryFactory = new QueryFactory(connection, compiler);

            return queryFactory;
        }
    }
}
