using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace citr.Models
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> Employees { get; }

        void SaveEmployee(Employee employee);

        Employee DeleteEmployee(int employeeId);

        Employee GetEmployee(int id);
    }
}
