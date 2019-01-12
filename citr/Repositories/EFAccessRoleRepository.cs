using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace citr.Models
{
    public class EFAccessRoleRepository : IAccessRoleRepository
    {
        private ApplicationDbContext context;

        public EFAccessRoleRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<AccessRole> Roles =>  
            context.AccessRoles;

        public void SaveRole(AccessRole role)
        {
            if (role.ID == 0)
            {
                context.AccessRoles.Add(role);
            }
            else
            {
                context.Entry(role).State = EntityState.Modified;
            }
            context.SaveChanges();
        }

        public AccessRole DeleteRole(int roleId)
        {
            AccessRole dbEntry = context.AccessRoles.FirstOrDefault(r => r.ID.Equals(roleId));
            if (dbEntry != null)
            {
                context.AccessRoles.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }
    }
}
