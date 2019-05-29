using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace citr.Models
{
    public class EFRequestRepository : IRequestRepository
    {
        private ApplicationDbContext context;

        public EFRequestRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Request> Requests =>
            context.Requests
            .Include(r => r.History)
            .Include(r => r.Author)
            .Include(r => r.Details)
                .ThenInclude(d => d.Resource)
                .ThenInclude(r => r.OwnerEmployee)
             .Include(r => r.Details)
                .ThenInclude(d => d.Resource)
                .ThenInclude(r => r.Category)
            .Include(r => r.Details)
                .ThenInclude(d => d.Role)
            .Include(d => d.Details)
                .ThenInclude(d => d.EmployeeAccess)
             .Include(r => r.Details)
                .ThenInclude(d => d.ResourceOwner)
            .Include(r => r.Details)
                .ThenInclude(d => d.Ticket);

        public IQueryable<RequestDetail> RequestsDetails =>
            context.RequestDetail
            .Include(r => r.EmployeeAccess)
            .Include(r => r.Resource)
            .Include(r => r.Role)
            .Include(r => r.Ticket);
    }
}
