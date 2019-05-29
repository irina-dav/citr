using System.Collections.Generic;

namespace citr.Models.ViewModels
{
    public class ResultUserUpdate
    {
        public int SearchedAccountsCount { get; set; }
        public int UpdatedUserCount { get; set; }
        public int NewUserCount { get; set; }
        public int NotValidAccountCount { get; set; }
        public List<string> NotValidAccounts { get; set; } = new List<string>();
        public List<EmployeeViewModel> NewEmployees { get; set; } = new List<EmployeeViewModel>();
        public List<EmployeeViewModel> UpdatedEmployees { get; set; } = new List<EmployeeViewModel>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
