using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace Identity_UI.Models;

public class LoginViewModel
{
    [Required]
    public string UserName { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string retunrUrl { get; set; }
    public IList<AuthenticationScheme> ExternalLoginSchemes { get; set; }

}