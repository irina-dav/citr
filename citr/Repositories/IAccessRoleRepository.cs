using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models
{
    public interface IAccessRoleRepository
    {
        IEnumerable<AccessRole> Roles { get; }

        void SaveRole(AccessRole role);

        //void AddRequestEvent(Request request, string textEvent);
    }
}
