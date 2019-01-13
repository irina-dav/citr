using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models
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
            .Include(r => r.Details)
                .ThenInclude(d => d.Resource)
                .ThenInclude(r => r.Category)
            .Include(r => r.Details)
                .ThenInclude(d => d.Role)
            .Include(d => d.Details)
                .ThenInclude(d => d.EmployeeAccess);
        //.Include(r => r.EmployeeAccesses)
        //.ThenInclude(r => r.Employee);


        public void SaveRequest(Request request)
        {
            if (request.RequestID == 0)
            {
                context.Requests.Add(request);
            }
            else
            {

                var fromDb = context.Requests
                 .Include(r => r.Details)
                 .Include(r => r.History)
                 .SingleOrDefault(x => x.RequestID.Equals(request.RequestID));

                fromDb.Comment = request.Comment;
                fromDb.ChangeDate = request.ChangeDate;
                fromDb.State = request.State;
                context.Entry(fromDb).State = EntityState.Modified;

                foreach (var d in fromDb.Details)
                {
                    context.Entry(d).State = EntityState.Deleted;
                }
                fromDb.Details.AddRange(new List<RequestDetail>(request.Details));
            }
            context.SaveChanges();
        }

        public void SaveRequestDetail(RequestDetail rd)
        {
            /* var fromDb = context.Requests
                 .Include(r => r.Details)
                 .Include(r => r.History)
                 .SingleOrDefault(x => x.Details.Any(d => d.ID == rd.ID));
             context.Entry(rd).State = EntityState.Modified;
             */
            var dbEntry = context.RequestDetail.First(d => d.ID == rd.ID);
            context.Entry(dbEntry).State = EntityState.Modified;
            context.SaveChanges();
        }
    }
}
