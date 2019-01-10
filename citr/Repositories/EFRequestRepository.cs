using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public class EFRequestRepository : IRequestRepository
    {
        private ApplicationDbContext context;

        public EFRequestRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<Request> Requests =>
            context.Requests
            .Include(r => r.History)
            .Include(r => r.ResourceAccesses)
            .ThenInclude(r => r.Resource)            
            .Include(r => r.EmployeeAccesses)
            .ThenInclude(r => r.Employee);

  
        public void SaveRequest(Request request)
        {
            if (request.RequestID == 0)
            {               
                context.Requests.Add(request);
            }
            else
            {
                //context.AttachRange(request.EmployeeAccesses.Select(l => l.Employee));
                
               var fromDb = context.Requests
                     .Include(r => r.ResourceAccesses)
                     .ThenInclude(r => r.Resource)
                     .Include(r => r.History)
                     .Include(r => r.EmployeeAccesses)
                     .ThenInclude(r => r.Employee).SingleOrDefault(x => x.RequestID.Equals(request.RequestID));

                fromDb.Comment = request.Comment;
                fromDb.ChangeDate = request.ChangeDate;
                fromDb.State = request.State;
                context.Entry(fromDb).State = EntityState.Modified;

                foreach (var ea in fromDb.EmployeeAccesses)
                {
                    context.Entry(ea).State = EntityState.Deleted;
                }
                fromDb.EmployeeAccesses.AddRange(new List<EmployeeAccess>(request.EmployeeAccesses));
                foreach (var ra in fromDb.ResourceAccesses)
                {
                    context.Entry(ra).State = EntityState.Deleted;
                }
                fromDb.ResourceAccesses.AddRange(new List<ResourceAccess>(request.ResourceAccesses));
                
                /*if (fromDb.History != null)
                {
                    foreach (var hr in fromDb.History)
                    {
                        context.Entry(hr).State = EntityState.Deleted;
                    }
                    fromDb.History.AddRange(new List<HistoryRow>(request.History));
                }*/
               // context.Entry(request).State = EntityState.Modified;
            }
            context.SaveChanges();
        }
    }
}
