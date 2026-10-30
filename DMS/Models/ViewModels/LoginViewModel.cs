using System.ComponentModel.DataAnnotations;

namespace DMS.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Salary Code is required")]
    [Display(Name = "Salary Code")]
    public string SalaryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
