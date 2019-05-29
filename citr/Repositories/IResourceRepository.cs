using System.Collections.Generic;

namespace citr.Models
{
    public interface IResourceRepository
    {
        IEnumerable<Resource> Resources { get; }

        void SaveResource(Resource resource);

        Resource DeleteResource(int resourceId);

        Resource GetResource(int id);
    }
}
