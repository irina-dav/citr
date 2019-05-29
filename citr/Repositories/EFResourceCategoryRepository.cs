using citr.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace citr.Models
{
    public class EFResourceCategoryRepository : IResourceCategoryRepository
    {
        private ApplicationDbContext context;

        public EFResourceCategoryRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<ResourceCategory> Categories
            => context.ResourceCategories;

        public void SaveCategory(ResourceCategory category)
        {
            if (category.ID == 0)
            {
                context.ResourceCategories.Add(category);
            }
            else
            {
                context.Entry(category).State = EntityState.Modified;
            }
            context.SaveChanges();
        }

        public ResourceCategory DeleteCategory(int categoryId)
        {
            ResourceCategory dbEntry = context.ResourceCategories.FirstOrDefault(r => r.ID == categoryId);
            if (dbEntry != null)
            {
                context.ResourceCategories.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }
    }
}
