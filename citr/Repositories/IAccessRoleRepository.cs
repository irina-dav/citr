using System.Collections.Generic;

namespace citr.Models
{
    public interface IAccessRoleRepository
    {
        IEnumerable<AccessRole> Roles { get; }

        void SaveRole(AccessRole role);

        AccessRole DeleteRole(int roleId);
    }
}
