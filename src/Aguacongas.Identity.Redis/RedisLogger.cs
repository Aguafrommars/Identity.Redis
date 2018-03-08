using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace Aguacongas.Identity.Redis
{
    public class RedisLogger : TextWriter
    {
        private readonly ILogger<RedisLogger> _logger;

        public override Encoding Encoding => throw new NotImplementedException();

        public RedisLogger(ILogger<RedisLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override void WriteLine(string format, object arg0)
        {
            _logger.LogTrace(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            _logger.LogTrace(format, arg0, arg1) ;
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            _logger.LogTrace(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            _logger.LogTrace(format, arg);
        }

        public override void WriteLine(string value)
        {
            _logger.LogTrace(value);
        }
    }
}
