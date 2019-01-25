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

namespace citr.Services
{
    public class NotificationService
    {
        private readonly IMailService mailService;
        private readonly IConfiguration configuration;
        private readonly IViewRenderService viewRenderService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public NotificationService(IMailService mailService, IConfiguration configuration, IViewRenderService viewRenderService, IHttpContextAccessor httpContextAccessor)
        {
            this.mailService = mailService;
            this.configuration = configuration;
            this.viewRenderService = viewRenderService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task SendToOTRSAsync(Request req)
        {
            string baseUrl = configuration.GetSection("AppSettings")["BaseUrl"].ToString();
            //string otrsEmail = configuration.GetSection("AppSettings")["OTRSEmail"].ToString();
            string otrsEmail = "i.davydenko@pharmasyntez.com";
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
                //System.IO.File.WriteAllText(@"d:/temp/test.html", viewHtml);

                await mailService.SendEmailAsync("i.davydenko@pharmasyntez.com", otrsEmail, $"Заявка на доступ к {res.Name} [{req.RequestID}|{res.ResourceID}]", viewHtml);
                //await mailService.SendEmailAsync(req.Author.Email, otrsEmail, $"Заявка на доступ к {res.Name} [{req.RequestID | res.ResourceID}]", viewHtml);               
            }
        }

        
        public async Task SendToApprovers(Request req)
        {
            //string fromEmail = configuration.GetSection("mail")["Email"].ToString();
            string fromEmail = "citr@pharmasyntez.com";
            string baseUrl = configuration.GetSection("AppSettings")["BaseUrl"].ToString();

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
               // System.IO.File.WriteAllText($"d:/temp/{approver.FullName.Replace(" ", "_")}.html", viewHtml);
                //await mailService.SendEmailAsync(fromEmail, approver.Email, $"Согласование заявки на доступ №{req.RequestID}", viewHtml);
                await mailService.SendEmailAsync(fromEmail, "i.davydenko@pharmasyntez.com", $"Согласование заявки на доступ №{req.RequestID}", viewHtml);
            }           
        }
    }
}
