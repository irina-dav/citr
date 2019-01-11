using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using citr.Models;

namespace citr.Repositories
{
    public interface IResourceCategoryRepository
    {
        IEnumerable<ResourceCategory> Categories { get; }

        void SaveCategory(ResourceCategory category);

        ResourceCategory DeleteCategory(int categoryId);
    }
}

