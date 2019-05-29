using citr.Models;
using System.Collections.Generic;

namespace citr.Repositories
{
    public interface IResourceCategoryRepository
    {
        IEnumerable<ResourceCategory> Categories { get; }

        void SaveCategory(ResourceCategory category);

        ResourceCategory DeleteCategory(int categoryId);
    }
}

