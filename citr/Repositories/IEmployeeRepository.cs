using System.Collections.Generic;

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
