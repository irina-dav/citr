using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public interface IRequestRepository
    {
        IEnumerable<Request> Requests { get; }

        void SaveRequest(Request request);

        //void AddRequestEvent(Request request, string textEvent);
    }
}
