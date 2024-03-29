﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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
            => context.Resources
                .Include(r => r.OwnerEmployee)
                .Include(r => r.Category)
                .Include(r => r.History)
                    .ThenInclude(h => h.AuthorEmployee)
                .Include(r => r.Roles);

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

        public Resource GetResource(int id)
        {
            return Resources.SingleOrDefault(r => r.ResourceID == id);
        }

        public void SaveResource(Resource resource)
        {
            if (resource.ResourceID == 0)
            {
                context.Resources.Add(resource);
            }
            else
            {
                Resource fromDb = context.Resources
                   .Include(r => r.OwnerEmployee)
                   .Include(r => r.Category)
                   .Include(r => r.History)
                   .Include(r => r.Roles)
                   .SingleOrDefault(x => x.ResourceID.Equals(resource.ResourceID));
                fromDb.Name = resource.Name;
                fromDb.ChangeDate = resource.ChangeDate;
                fromDb.Description = resource.Description;
                fromDb.CategoryID = resource.CategoryID;
                fromDb.OwnerEmployeeID = resource.OwnerEmployeeID;
                context.Entry(fromDb).State = EntityState.Modified;
                foreach (AccessRole r in fromDb.Roles)
                {
                    context.Entry(r).State = EntityState.Deleted;
                }
                fromDb.Roles.AddRange(new List<AccessRole>(resource.Roles));
            }
            context.SaveChanges();
        }
    }
}
