using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace citr.Models
{
    public class EFResourceRepository : IResourceRepository
    {
        private ApplicationDbContext context;

        public EFResourceRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<Resource> Resources 
            => context.Resources.Include(r => r.OwnerEmployee).Include(r => r.Category).Include(r => r.History).Include(r => r.Roles);
     
        public Resource DeleteResource(int resourceId)
        {
            Resource dbEntry = context.Resources.FirstOrDefault(r => r.ResourceID.Equals(resourceId));
            if (dbEntry != null)
            {
                context.Resources.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }

        public void SaveResource(Resource resource)
        {
            if (resource.ResourceID == 0)
            {
                context.Resources.Add(resource);
            }
            else
            {
                context.Entry(resource).State = EntityState.Modified;
            }
            context.SaveChanges();
        }
    }
}
