using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using citr.Models;
using citr.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace citr.Services
{
    internal class TicketUpdateService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;        
        private IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        public TicketUpdateService(ILogger<TicketUpdateService> logger, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;
            configuration = config;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TicketUpdateService is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("TicketUpdateService is working.");
            using (var scope = serviceProvider.CreateScope())
            {
                /*var otrsService = scope.ServiceProvider.GetService<OTRSService>();
                otrsService.UpdateRequestDetails();
                otrsService.UpdateTickets();*/
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TicketUpdateService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
