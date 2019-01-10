using RequestsAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Services
{
    public interface IHistoryable
    {
        List<HistoryRow> History { get; set; }

        int ObjectID { get;}
    }

    public class HistoryService
    {
        private readonly ApplicationDbContext context;
        private readonly ILdapService ldapService;

        public HistoryService(ApplicationDbContext ctx, ILdapService ldapSrv)
        {
            context = ctx;
            ldapService = ldapSrv;
        }

        public void AddRow(IHistoryable obj, string text)
        {
            if (obj.History == null)
                obj.History = new List<HistoryRow>();

             obj.History.Add(new HistoryRow()
             {
                 AuthorEmployeeID = ldapService.GetUserEmployee().EmployeeID,
                 Date = DateTime.Now,
                 Text = text
             });   
            context.SaveChanges();
        }       
    }
}
