using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace citr.Models
{
    public class EFAcessRoleRepository : IAccessRoleRepository
    {
        private ApplicationDbContext context;

        public EFAcessRoleRepository(ApplicationDbContext ctx)
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
    }
}
