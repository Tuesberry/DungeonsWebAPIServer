using Microsoft.Extensions.Options;

namespace TuesberryAPIServer.Services
{
    public class GameDb : IGameDb
    {
        readonly ILogger<GameDb> _logger;
        readonly IOptions<DbConfig> _dbConfig;
        public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> options) 
        { 
            _logger = logger;
            _dbConfig = options;
        }

        public void Dispose()
        {

        }
    }
}
