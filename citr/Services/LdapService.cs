using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using citr.Models;
using citr.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace citr.Services
{
    public interface ILdapService
    {
        ResultUserUpdate UpdateEmployees();
        AppUser Login(string username, string password);
        string GetUserDisplayInfo();
        Employee GetUserEmployee();
    }

    public class LdapService : ILdapService
    {
        private const string MemberOfAttribute = "memberOf";
        private const string DisplayNameAttribute = "displayName";
        private const string SAMAccountNameAttribute = "sAMAccountName";
        private const string TitleAttribute = "title";
        private const string MailAttribute = "mail";

        private readonly LdapConfig config;
        private readonly LdapConnection connection;
        private IEmployeeRepository employeesRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        ILogger<LdapService> logger;

        public LdapService(IOptions<LdapConfig> cfg, IEmployeeRepository emplRepo, IHttpContextAccessor httpCtxAccessor, ILogger<LdapService> logger)
        {
            httpContextAccessor = httpCtxAccessor;
            employeesRepository = emplRepo;
            config = cfg.Value;
            this.logger = logger;
            connection = new LdapConnection
            {
                SecureSocketLayer = false
            };           
        }


        public AppUser Login(string username, string password)
        {
            connection.Connect(config.Url, LdapConnection.DEFAULT_PORT);
            connection.Bind(config.BindDn, config.BindCredentials);

            var searchFilter = string.Format(config.AuthFilter, username);
            var result = connection.Search(
                config.SearchBase,
                LdapConnection.SCOPE_SUB,
                searchFilter,
                new[] { MemberOfAttribute, DisplayNameAttribute, SAMAccountNameAttribute, TitleAttribute, MailAttribute },
                false
            );

            try
            {
                var user = result.Next();
                if (user != null)
                {
                    connection.Bind(user.DN, password);
                    if (connection.Bound)
                    {
                        return new AppUser
                        {
                            DisplayName = user.getAttribute(DisplayNameAttribute)?.StringValue ?? "",
                            Username = user.getAttribute(SAMAccountNameAttribute).StringValue,
                            Email = user.getAttribute(MailAttribute)?.StringValue ?? "",
                            Position = user.getAttribute(TitleAttribute)?.StringValue ?? ""
                            //IsAdmin = user.getAttribute(MemberOfAttribute).StringValueArray.Contains(config.AdminCn)
                        };
                    }
                }
            }
            catch
            {
                throw new Exception("Введён неправильный логин или пароль");
            }
            connection.Disconnect();
            return null;
        }

        public string GetUserDisplayInfo()
        {
            string userName = "...";
            Employee userEmployee = GetUserEmployee();
            if (userEmployee != null)
            {
                userName = $"{userEmployee.FullName}";
            }            
            return userName;
        }

        public Employee GetUserEmployee()
        {
            string account = httpContextAccessor.HttpContext.User.Identity.Name;
            return employeesRepository.Employees.FirstOrDefault(e => e.Account.Equals(account, StringComparison.InvariantCultureIgnoreCase));            
        }

        public ResultUserUpdate UpdateEmployees()
        {
            StringBuilder sb = new StringBuilder();
            ResultUserUpdate result = new ResultUserUpdate();            
            try
            {
                connection.Connect(config.Url, LdapConnection.DEFAULT_PORT);
                connection.Bind(config.BindDn, config.BindCredentials);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.ToString());
                logger.LogError(ex.ToString());
                return result;
            }

            try
            {
                foreach (string orgUnit in config.OrgUnits)
                {
                    var resultSearch = connection.Search(
                    orgUnit,
                    LdapConnection.SCOPE_SUB,
                    config.SearchFilter,
                    new[] { MemberOfAttribute, DisplayNameAttribute, SAMAccountNameAttribute, TitleAttribute, MailAttribute },
                    false
                    ).ToList();

                    foreach (var user in resultSearch)
                    {                        
                        if (user != null)
                        {
                            result.SearchedAccountsCount++;
                            string account = user.getAttribute(SAMAccountNameAttribute).StringValue;
                            string fullName = user.getAttribute(DisplayNameAttribute)?.StringValue ?? "";
                            string email = user.getAttribute(MailAttribute)?.StringValue ?? "";
                            string position = user.getAttribute(TitleAttribute)?.StringValue ?? "";
                            if (fullName == "" || email == "" || position == "")
                            {
                                result.NotValidAccountCount++;
                                result.NotValidAccounts.Add(account);                                
                                continue;
                            }

                            if (!employeesRepository.Employees.Any(em => em.Account.Equals(account, comparisonType: StringComparison.InvariantCultureIgnoreCase)))
                            {

                                Employee employee = new Employee
                                {
                                    Account = account,
                                    Email = email,
                                    FullName = fullName,
                                    Position = position
                                };

                                try
                                {
                                    employeesRepository.SaveEmployee(employee);
                                    result.NewUserCount++;
                                    result.NewEmployees.Add(new EmployeeViewModel(employee));
                                }
                                catch (Exception ex)
                                {
                                    result.Errors.Add($"Не удалось добавить сотрудника {account}: {ex.ToString()}");
                                    logger.LogError(ex.ToString());
                                }
                            }
                            else
                            {
                                Employee employee = employeesRepository.Employees.First(e => e.Account == account);
                                bool updated = false;
                                if (employee.Email != email)
                                {
                                    employee.Email = email;
                                    updated = true;
                                }
                                if (employee.FullName != fullName)
                                {
                                    employee.FullName = fullName;
                                     updated = true;
                                }
                                if (employee.Position != position)
                                {
                                    employee.Position = position;
                                    updated = true;
                                }
                                if (updated)
                                {
                                    result.UpdatedUserCount++;
                                    result.UpdatedEmployees.Add(new EmployeeViewModel(employee));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.ToString());
                logger.LogError(ex.ToString());
            }
            connection.Disconnect();
            return result;
        }       
    }
}