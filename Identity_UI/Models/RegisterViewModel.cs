using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Identity_UI.Models;

public class RegisterViewModel
{
    [Required]
    [Remote("IsDuplicateUsername", "Account",HttpMethod = "POST",AdditionalFields = "__RequestVerificationToken")]
    public string UserName { get; set; }
    [Required]
    [EmailAddress]
    [Remote("IsDuplicateEmail", "Account")]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }


}