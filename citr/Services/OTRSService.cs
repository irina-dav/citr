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

        public void UpdateRequestDetails()
        {         
            try
            {                    
                var approvedRequests = requestRepository.Requests.Where(r => r.State == RequestState.Approved);
                var detailsToUpdate = approvedRequests.SelectMany(d => d.Details).Where(d => d.ApprovingResult == ResourceApprovingResult.Approved && !d.TicketID.HasValue);

                logger.LogInformation($"Found RequestDetails for update: {detailsToUpdate.Count()}");

                if (detailsToUpdate.Count() == 0)
                    return;

                using (var connection = new MySqlConnection(configuration.GetConnectionString("OTRS")))
                {
                    foreach (RequestDetail det in detailsToUpdate)
                    {
                        Ticket ticketWithNumber = SearchTicketOnlyNumber(det.ID.ToString(), connection);
                        if (ticketWithNumber != null)
                        {
                            if (context.Tickets.Find(ticketWithNumber.TicketID) == null)
                            {
                                context.Tickets.Add(ticketWithNumber);
                                //context.SaveChanges();
                            }
                            det.TicketID = ticketWithNumber.TicketID;
                            
                            //requestRepository.SaveRequestDetail(det);
                            logger.LogInformation($"Updated RequestDetail {det.ID}, ticketID: {det.TicketID}");
                        }
                    }
                    context.SaveChanges();
                }                   
            }            
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }

        public void UpdateTickets()
        {
            try
            {
                var ticketsEmptyEndDate = context.Tickets.Where(t => !t.EndDate.HasValue).Select(t => t.TicketID);
             
                logger.LogInformation($"Found Tickets with empty EndDate: {ticketsEmptyEndDate.Count()}");

                if (ticketsEmptyEndDate.Count() == 0)
                    return;

                using (var connection = new MySqlConnection(configuration.GetConnectionString("OTRS")))
                {
                    foreach (long ticketId in ticketsEmptyEndDate)
                    {
                        TicketEndInfo ticketWithEndDate = SearchTicketEndInfo(ticketId, connection);
                        if (ticketWithEndDate != null)
                        {
                            var ticketEntry = context.Tickets.Find(ticketId);
                            ticketEntry.EndDate = ticketWithEndDate.EndDate;
                            ticketEntry.EndByUser = ticketWithEndDate.EndByUser;
                            logger.LogInformation($"Updated Ticket {ticketEntry.TicketID}");
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }

        class TicketEndInfo
        {
            public DateTime? EndDate { get; set; }
            public string EndByUser { get; set; }
        }

        private TicketEndInfo SearchTicketEndInfo(long ticketId, MySqlConnection connection)
        {
            string endUser = "";
            DateTime? endDate = null;
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                var command = new MySqlCommand("SELECT ticket_history.id, concat_ws(' ', users.last_name, users.first_name) user, ticket_history.change_time time FROM ticket_history " +
                                                "LEFT JOIN users ON ticket_history.change_by = users.id " +
                                                "WHERE ticket_history.ticket_id =@ticketId && RIGHT(name, 19) = 'closed successful%%'", connection);
                command.Parameters.AddWithValue("ticketId", ticketId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        endUser = reader["user"].ToString();
                        endDate = DateTime.Parse(reader["time"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            if (!string.IsNullOrEmpty(endUser) && endDate.HasValue)
            {
                return new TicketEndInfo()
                {
                    EndDate = endDate.Value,
                    EndByUser = endUser
                };
            }
            else
                return null;

        }

        private Ticket SearchTicketOnlyNumber(string detailText, MySqlConnection connection)
        {
            string tn = "";
            int id = 0;
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                var command = new MySqlCommand("SELECT id, tn, title FROM otrs2.ticket where title like CONCAT('%', @number, '%') LIMIT 1;", connection);
                command.Parameters.AddWithValue("number", detailText);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tn = reader["tn"].ToString();
                        id = int.Parse(reader["id"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
            if (!string.IsNullOrEmpty(tn) && id > 0)
                return new Ticket() { TicketID = id, TicketNumber = tn };
            else
                return null;
        }
    }
}
