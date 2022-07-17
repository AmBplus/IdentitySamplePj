using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Identity_UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Identity_UI.Controllers;

/// <summary>
/// 
/// </summary>
public class AccountController : Controller
{
    // Properties
    private SignInManager<IdentityUser> _signInManager;
    private UserManager<IdentityUser> _userManager;
    public RegisterViewModel RegisterView { get; set; }
    public LoginViewModel LoginView { get; set; }
    private IEmailSender _emailSender;

    // Ctor
    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
    }


    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LogOutAsync(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        if (CheckIsLocalAddress(returnUrl))
            return Redirect(returnUrl!);
        return RedirectToAction("Index", "Home");
    }

    #region Login

    //-------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        ViewData["returnUrl"] = returnUrl;
        LoginView = new LoginViewModel()
        {
            ExternalLoginSchemes = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };
        return View(LoginView);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostLoginAsync(LoginViewModel loginViewModel, string? returnUrl = null)
    {
        ViewData["returnUrl"] = returnUrl;
        LoginView = loginViewModel;
        if (ModelState.IsValid)
        {

            var result = await _signInManager.PasswordSignInAsync(LoginView.UserName, LoginView.Password,
                LoginView.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                if (CheckIsLocalAddress(returnUrl))
                    return Redirect(returnUrl!);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Multiple Enter Wrong Password  Or Username Wait To Unlock");
                return View("Login", LoginView);
            }

            ModelState.AddModelError("", "Enter Wrong Username Or Password");
        }

        return View("Login", LoginView);
    }

    #endregion

    //-------------------------------------------------------------------

    #region Register

    public IActionResult Register(string returnUrl = null)
    {
        ViewData["returnUrl"] = returnUrl;
        return View(RegisterView);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostRegisterAsync(RegisterViewModel registerView, string? returnUrl = null)
    {
        ViewData["returnUrl"] = returnUrl;
        RegisterView = registerView;
        if (ModelState.IsValid)
        {
            var user = new IdentityUser()
            {
                UserName = RegisterView.UserName,
                Email = RegisterView.Email,
            };
            var result = await _userManager.CreateAsync(user, RegisterView.Password);
            if (result.Succeeded)
            {
                await SendEmailAsync(user, RegisterView);
                if (CheckIsLocalAddress(returnUrl))
                    return Redirect(returnUrl!);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }

        return View("Register", model: RegisterView);
    }



    #endregion

    //  Methods

    // Ajax Validation

    /// <summary>
    /// check DuplicatedUsername
    /// </summary>
    /// <param name="entity">Enter username</param>
    /// <returns>return json result</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IsDuplicateUsername(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return Json(true);
        return Json("This User Can't be Used , Try Another");
    }

    /// <summary>
    /// check duplicated email
    /// </summary>
    /// <param name="entity">enter email</param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IsDuplicateEmail(string email)
    {
        var userEmail = await _userManager.FindByEmailAsync(email);
        if (userEmail == null) return Json(true);
        return Json(
            "     This email Can't be Used ,maybe you already registered try forget password or Try Another email");
    }

    /// <summary>
    /// Check Address Is Local 
    /// </summary>
    /// <param name="address">Enter Address</param>
    /// <returns>Boolean Result</returns>

    private bool CheckIsLocalAddress(string? address)
    {
        if (!string.IsNullOrWhiteSpace(address) && Url.IsLocalUrl(address))
        {
            return true;
        }

        return false;
    }

    private async Task SendEmailAsync(IdentityUser user, RegisterViewModel Input)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Action("ConfirmEmail",
            "Account",
            values: new { username = user.UserName, Token = code },
            protocol: Request.Scheme);
        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
    }

    public async Task<IActionResult> ConfirmEmailAsync(string username, string token)
    {
        if (username == null || token == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{username}'.");
        }

        token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return Content(result.Succeeded ? "Email Confirm" : "Email Not Confirm");
    }

    /// <summary>
    /// Accept External name of Provider Like (Google , Tweeter)
    /// </summary>
    /// <param name="providerName"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult ExternalLogin(string providerName , string returnUrl)
    {
        var externalLoginCallBack = Url.Action("ExternalLoginAccepter", 
            "Account", new { ReturnUrl = returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(providerName, externalLoginCallBack);
        return new ChallengeResult(providerName, properties);
    }
    /// <summary>
    /// Accept External Provider Info Like (Google , Tweeter)
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <param name="remoteError"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
   
    public async Task<IActionResult> ExternalLoginAccepter(string? returnUrl = null,string? remoteError = null)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl)) returnUrl = Url.Content("/");
        LoginView = new LoginViewModel()
        {
            ExternalLoginSchemes = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            retunrUrl = returnUrl,
        };
        if (remoteError != null)
        {
            ModelState.AddModelError("External Login From Provider Failed", $"External Login From Provider Failed\n {remoteError}");
            return View("Login", LoginView);
        }
        var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo == null)
        {
            ModelState.AddModelError("External Login From Provider Failed", "External Login Failed ,Some Problem Occurred");
            return View("Login", LoginView);
        }
        var signInResult = await _signInManager.ExternalLoginSignInAsync
            (externalLoginInfo.LoginProvider,externalLoginInfo.ProviderKey,false,true);
        if (signInResult.Succeeded) return Redirect(returnUrl);
        var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            ModelState.AddModelError("",$"Can't Get Info From {externalLoginInfo.LoginProvider}, Contact To Us");
            return View("Login", LoginView);
        }
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var username = email.Split('@')[0];
            if (username.Length > 10) username = username.Substring(0, 10);
            user = new IdentityUser()
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
            };
            await _userManager.CreateAsync(user);
        }
        await _userManager.AddLoginAsync(user, externalLoginInfo);
        await _signInManager.SignInAsync(user, false);
        return Redirect(returnUrl);
    }
}