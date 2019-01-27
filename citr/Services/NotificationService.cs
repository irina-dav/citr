using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using citr.Models;
using citr.Models.ViewModels;
using citr.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace citr.Services
{
    public class NotificationService
    {
        private readonly IMailService mailService;
        private readonly MailConfig config;
        private readonly IViewRenderService viewRenderService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public NotificationService(IMailService mailService, IOptions<MailConfig> config, IViewRenderService viewRenderService, IHttpContextAccessor httpContextAccessor)
        {
            this.mailService = mailService;
            this.config = config.Value;
            this.viewRenderService = viewRenderService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task SendToOTRSAsync(Request req, string baseUrl)
        {            
            foreach (var resGroup in req.Details.Where(d => d.ApprovingResult == ResourceApprovingResult.Approved).GroupBy(d => d.Resource))
            {
                Resource res = resGroup.Key;
                EmailViewModel model = new EmailViewModel()
                {
                    Recipient = req.Author,
                    Request = req,
                    Resources = new List<Resource>() { res },
                    Details = resGroup.ToList(),
                    Url = $"{baseUrl}/Request/Open/{req.RequestID}"
                };

                var viewHtml = await viewRenderService.RenderToStringAsync("Email/OTRS", model);
                // req.Author.Email
                await mailService.SendEmailAsync("i.davydenko@pharmasyntez.com", config.OTRSEmail, $"Заявка на доступ к {res.Name} [{req.RequestID}|{res.ResourceID}]", viewHtml);         
            }
        }
        
        public async Task SendToApprovers(Request req, string baseUrl)
        {
            foreach (var g in req.Details.GroupBy(d => d.Resource.OwnerEmployee))
            {
                Employee approver = g.Key;
                var details = g.ToList();               
                EmailViewModel model = new EmailViewModel()
                {
                    Recipient = approver,
                    Request = req,
                    Resources = details.Select(d => d.Resource).Distinct().ToList(),
                    Details = details,
                    Url = $"{baseUrl}/Request/Open/{req.RequestID}"
                };
                var viewHtml = await viewRenderService.RenderToStringAsync("Email/Approve", model);
                //approver.Email
                await mailService.SendEmailAsync(config.FromEmail, "i.davydenko@pharmasyntez.com", $"Согласование заявки на доступ №{req.RequestID}", viewHtml);
            }           
        }
    }
}
