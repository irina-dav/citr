using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using citr.Repositories;
using citr.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace citr.Services
{
    public class OTRSService
    {
        private IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IRequestRepository requestRepository;
        private readonly ApplicationDbContext context;

        public OTRSService(IConfiguration config, ILogger<OTRSService> logger, IRequestRepository requestRepo, ApplicationDbContext context)
        {
            this.configuration = config;
            this.logger = logger;
            requestRepository = requestRepo;
            this.context = context;
        }

        public string GetTicketUrl(string ticketNumber)
        {
            return string.Format(configuration.GetSection("AppSettings")["OTRSTicketUrlTemplate"].ToString(), ticketNumber);
        }

        public void UpdateTickets()
        {         
            try
            {                    
                var approvedRequests = requestRepository.Requests.Where(r => r.State == RequestState.Approved);
                var detailsToUpdate = approvedRequests.SelectMany(d => d.Details).Where(d => d.ApprovingResult == ResourceApprovingResult.Approved && string.IsNullOrEmpty(d.TicketNumber));

                logger.LogInformation($"Found RequestDetails for update: {detailsToUpdate.Count()}");

                if (detailsToUpdate.Count() == 0)
                    return;
                using (var connection = new MySqlConnection(configuration.GetConnectionString("OTRS")))
                {
                    foreach (RequestDetail det in detailsToUpdate)
                    {
                        string ticketNumber = SearchTicket(det.ID.ToString(), connection);
                        if (!string.IsNullOrEmpty(ticketNumber))
                        {
                            det.TicketNumber = ticketNumber;
                            //context.SaveChanges();
                            requestRepository.SaveRequestDetail(det);
                            logger.LogInformation($"Updated RequestDetail {det.ID}, ticketNumer: {det.TicketNumber}");
                        }
                    }
                }                   
            }            
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }

        private string SearchTicket(string number, MySqlConnection connection)
        {
            string tn = null;
            try
            {

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                var command = new MySqlCommand("SELECT * FROM otrs2.ticket where title like CONCAT('%', @number, '%') LIMIT 1;", connection);
                command.Parameters.AddWithValue("number", number);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tn = reader["tn"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            return tn;
        }
    }
}
