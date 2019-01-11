using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models
{
    public interface IResourceRepository
    {
        IEnumerable<Resource> Resources { get; }

        void SaveResource(Resource resource);

        Resource DeleteResource(int resourceId);
    }
}
