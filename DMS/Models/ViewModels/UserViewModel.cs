using System.ComponentModel.DataAnnotations;

namespace DMS.Models.ViewModels;

public class UserCreateViewModel
{
    [Required, MaxLength(50)]
    [Display(Name = "Salary Code")]
    public string SalaryCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Access Role")]
    public string AccessRole { get; set; } = "Plant";

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}

public class UserEditViewModel
{
    public long UserID { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Salary Code")]
    public string SalaryCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Display(Name = "Employee Name")]
    public string EmployeeName { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "New Password (leave blank to keep current)")]
    public string? NewPassword { get; set; }

    [Required]
    [Display(Name = "Access Role")]
    public string AccessRole { get; set; } = "Plant";

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}
