using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using citr.Models;

namespace citr.Services
{
    public interface ILdapService
    {
        string PopulateEmployees();
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

        public LdapService(IOptions<LdapConfig> cfg, IEmployeeRepository emplRepo, IHttpContextAccessor httpCtxAccessor)
        {
            httpContextAccessor = httpCtxAccessor;
            employeesRepository = emplRepo;
            config = cfg.Value;
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
                new[] { MemberOfAttribute, DisplayNameAttribute, SAMAccountNameAttribute },
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
                            DisplayName = user.getAttribute(DisplayNameAttribute).StringValue,
                            Username = user.getAttribute(SAMAccountNameAttribute).StringValue,
                            IsAdmin = user.getAttribute(MemberOfAttribute).StringValueArray.Contains(config.AdminCn)
                        };
                    }
                }
            }
            catch
            {
                throw new Exception("Login failed.");
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

        public string PopulateEmployees()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                connection.Connect(config.Url, LdapConnection.DEFAULT_PORT);
                connection.Bind(config.BindDn, config.BindCredentials);
            }
            catch (Exception ex)
            {
                sb.Append(ex.ToString());
                return sb.ToString();
            }

            try
            {
                var result = connection.Search(
                config.SearchBase,
                LdapConnection.SCOPE_SUB,
                config.SearchFilter,
                new[] { MemberOfAttribute, DisplayNameAttribute, SAMAccountNameAttribute, TitleAttribute, MailAttribute },
                false
            ).ToList();


                foreach (var user in result)
                // var user = result.Next();
                {
                    if (user != null)
                    {
                        string account = user.getAttribute(SAMAccountNameAttribute).StringValue;
                        string fullName = user.getAttribute(DisplayNameAttribute)?.StringValue ?? "";
                        string email = user.getAttribute(MailAttribute)?.StringValue ?? "";
                        string position = user.getAttribute(TitleAttribute)?.StringValue ?? "";
                        if (fullName == "" || email == "" || position == "")
                        {
                            sb.AppendLine($"У сотрудника {account} не указан один из атрибутов: DisplayName, должность, email");
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
                            }
                            catch(Exception ex)
                            {
                                sb.AppendLine($"Не удалось добавить сотрудника {account}: {ex.ToString()}");
                            }
                        }
                        else
                        {                            
                            Employee employee = employeesRepository.Employees.First(e => e.Account == account);
                            employee.Email = email;
                            employee.FullName = fullName;
                            employee.Position = position;                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append(ex.ToString());
            }
            connection.Disconnect();
            sb.AppendLine("Загрузка и обновление данных завершены");
            return sb.ToString();
        }       
    }
}