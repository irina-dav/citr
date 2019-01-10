using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RequestsAccess.Models
{

    /*public class EFFileShareResourceRepository : IFileShareResourceRepository
    {
        private ApplicationDbContext context;

        public EFFileShareResourceRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<FileShareResource> Resources 
            => context.Resources.OfType<FileShareResource>().Include(r => r.OwnerEmployee);
     
        public FileShareResource DeleteResource(int resourceId)
        {
            FileShareResource dbEntry = context.Resources.OfType<FileShareResource>().FirstOrDefault(r => r.ResourceID.Equals(resourceId));
            if (dbEntry != null)
            {
                context.Resources.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }

        public void SaveResource(FileShareResource resource)
        {
            if (resource.ResourceID.Equals(Guid.Empty))
            {
                context.Resources.Add(resource);
            }
            else
            {
                context.Entry(resource).State = EntityState.Modified;
                /*Resource dbEntry = context.Resources.FirstOrDefault(r => r.ResourceID == resource.ResourceID);
                if (dbEntry != null)
                {
                    dbEntry.Name = resource.Name;
                    dbEntry.OwnerEmployee = resource.OwnerEmployee;
                    dbEntry.Editor = resource.Editor;
                    dbEntry.Description = resource.Description;
                    dbEntry.Hidden = resource.Hidden;
                    dbEntry.ChangeDate = resource.ChangeDate;
                }*/
           // }
          //  context.SaveChanges();
      //  }
    //}
}