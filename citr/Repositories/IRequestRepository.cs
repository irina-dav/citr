using System.Linq;

namespace citr.Models
{
    public interface IRequestRepository
    {
        IQueryable<Request> Requests { get; }

        IQueryable<RequestDetail> RequestsDetails { get; }
    }
}
