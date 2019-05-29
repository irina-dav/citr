using citr.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace citr.Repositories
{
    public class EFEmployeesRepository : IEmployeeRepository
    {
        private ApplicationDbContext context;

        public EFEmployeesRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<Employee> Employees =>
            context.Employees.Include(e => e.UserRole);

        public Employee DeleteEmployee(int employeeId)
        {
            Employee dbEntry = context.Employees.FirstOrDefault(e => e.EmployeeID.Equals(employeeId));
            if (dbEntry != null)
            {
                context.Employees.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }

        public Employee GetEmployee(int id)
        {
            return Employees.FirstOrDefault(em => em.EmployeeID == id);
        }

        public void SaveEmployee(Employee employee)
        {
            if (employee.EmployeeID == 0)
            {
                context.Employees.Add(employee);
            }
            else
            {
                Employee dbEntry = context.Employees.FirstOrDefault(r => r.Account == employee.Account);
                if (dbEntry != null)
                {
                    dbEntry.Email = employee.Email;
                    dbEntry.FullName = employee.FullName;
                    dbEntry.Position = employee.Position;
                }
            }
            context.SaveChanges();
        }
    }
}
