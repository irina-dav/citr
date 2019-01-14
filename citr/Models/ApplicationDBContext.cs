using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace citr.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Resource> Resources { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<AccessRole> AccessRoles { get; set; }

        public DbSet<ResourceCategory> ResourceCategories { get; set; }

        public DbSet<RequestDetail> RequestDetail { get; set; }

        public DbSet<HistoryRow> History { get; set; }

        public DbSet<UserRole> UserRole { get; set; }

        //public DbSet<EmployeeAccess> EmployeeAccess { get; set; }

        //public DbSet<ResourceAccess> ResourceAccess { get; set; }

    }
}
