using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FourOneShot.Pi.API.Attributes;
using FourOneShot.Pi.Devices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FourOneShot.Pi.API.Controllers
{
    [ApiController]
    [Route("api/blinds")]
    public class BlindsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlindsController> _logger;
        private readonly DD2702H _blindRemote;

        public BlindsController(IConfiguration configuration, ILogger<BlindsController> logger, DD2702H blindRemote)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blindRemote = blindRemote ?? throw new ArgumentNullException(nameof(_blindRemote));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return NoContent();
        }

        [HttpGet]
        [Route("configuration")]
        public Dictionary<string, object> GetConfiguration()
        {
            var blindChannelMappings = _configuration.GetSection("BlindChannelMappings")?
                .Get<Dictionary<string, int>>();

            var result = new Dictionary<string, object>
            {
                { "BlindChannelMappings", blindChannelMappings }
            };

            return result;
        }

        [HttpGet]
        [Route("channel")]
        [SequentialExecution]
        public int GetChannel()
        {
            return _blindRemote.Channel;
        }

        [HttpPost]
        [Route("channel/{id}")]
        [SequentialExecution]
        public async Task SetChannel([FromRoute] int id)
        {
            await _blindRemote.SetChannel(id);
        }

        [HttpPost]
        [Route("reset")]
        [SequentialExecution]
        public async Task Reset()
        {
            await _blindRemote.Reset();
        }

        [HttpPost]
        [Route("open")]
        [SequentialExecution]
        public async Task Open()
        {
            await _blindRemote.Open();
        }

        [HttpPost]
        [Route("close")]
        [SequentialExecution]
        public async Task Close()
        {
            await _blindRemote.Close();
        }

        [HttpPost]
        [Route("stop")]
        [SequentialExecution]
        public async Task Stop()
        {
            await _blindRemote.Stop();
        }

        [HttpPost]
        [Route("pair")]
        [SequentialExecution]
        public async Task Pair()
        {
            await _blindRemote.Pair();
        }

        [HttpPost]
        [Route("channel/up")]
        [SequentialExecution]
        public async Task ChannelUp()
        {
            await _blindRemote.ChannelUp();
        }

        [HttpPost]
        [Route("channel/down")]
        [SequentialExecution]
        public async Task ChannelDown()
        {
            await _blindRemote.ChannelDown();
        }

        [HttpPost]
        [Route("channel/limit")]
        [SequentialExecution]
        public async Task ChannelLimit()
        {
            await _blindRemote.ChannelLimit();
        }
    }
}
