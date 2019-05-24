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
using Hangfire;

namespace citr.Services
{
    public class NotificationService
    {
        private readonly IMailService mailService;
        private readonly MailConfig config;
        private readonly IViewRenderService viewRenderService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IBackgroundJobClient backgroundJobs;

        public NotificationService(IMailService mailService, IOptions<MailConfig> config, IViewRenderService viewRenderService, IHttpContextAccessor httpContextAccessor, IBackgroundJobClient backgroundJobs)
        {
            this.mailService = mailService;
            this.config = config.Value;
            this.viewRenderService = viewRenderService;
            this.httpContextAccessor = httpContextAccessor;
            this.backgroundJobs = backgroundJobs;
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
                await mailService.SendEmailAsync(req.Author.Email, config.OTRSEmail, $"Заявка на доступ к {res.Name} [{req.RequestID}|{res.ResourceID}]", viewHtml);         
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
                //await mailService.SendEmailAsync(config.FromEmail, approver.Email, $"Согласование заявки на доступ №{req.RequestID}", viewHtml);
                backgroundJobs.Enqueue<MailService>(ms => ms.SendEmailAsync(config.FromEmail, approver.Email, $"Согласование заявки на доступ №{req.RequestID}", viewHtml));
            }           
        }
    }
}
