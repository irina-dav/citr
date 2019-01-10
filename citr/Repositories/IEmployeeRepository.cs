using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestsAccess.Models
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> Employees { get; }

        void SaveEmployee(Employee employee);

        Employee DeleteEmployee(int employeeId);
    }
}
