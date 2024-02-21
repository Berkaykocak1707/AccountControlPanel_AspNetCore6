using AccountControlPanel_AspNetCore6.Models;
using Business.Contracts;
using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Web;

namespace AccountControlPanel_AspNetCore6.Controllers
{
	public class HomeController : Controller
	{
		private readonly IServiceManager _serviceManager;
		private readonly Microsoft.AspNetCore.Identity.UserManager<CustomUser> _userManager;
		private readonly SignInManager<CustomUser> _signInManager;
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public HomeController(IServiceManager serviceManager, Microsoft.AspNetCore.Identity.UserManager<CustomUser> userManager, SignInManager<CustomUser> signInManager, IConfiguration configuration, IHttpClientFactory httpClientFactory)
		{
			_serviceManager = serviceManager;
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
		}

		[HttpGet]
		public async Task<bool> VerifyEmail(string email)
		{
			var apiKey = _configuration["AbstractApi:ApiKey"];
			var apiUrl = $"https://emailvalidation.abstractapi.com/v1/?api_key={apiKey}&email={System.Net.WebUtility.UrlEncode(email)}";

			var client = _httpClientFactory.CreateClient();
			var response = await client.GetStringAsync(apiUrl);
			var jsonObject = JObject.Parse(response);
			var isSmtpValidText = (string)jsonObject["is_smtp_valid"]["text"];

			if (isSmtpValidText == "TRUE")
				return true;
			else
				return false;
		}
		[Authorize]
		public async Task<IActionResult> Index()
		{
			var model = await _serviceManager.AuthService.GetOneUserForHomeAsync(User.Identity.Name);
			if (model.LockoutEnabled == false)
			{
				await _signInManager.SignOutAsync();
				return RedirectToAction("ForgotPassword", "Home", new
				{
					username = model.UserName
				});

			}
			return View(model);
		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update([FromForm] UserDto userDto, IFormFile? photo, bool deletePhoto)
		{
			if (ModelState.IsValid)
			{
				var currentUser = await _serviceManager.AuthService.GetOneUserAsync(User.Identity.Name);
				if (currentUser.UserName != userDto.UserName)
				{
					var user = await _serviceManager.AuthService.GetOneUserAsync(userDto.UserName);
					if (user != null)
					{
						TempData["Message"] = $"{userDto.UserName} is already taken.";
						return RedirectToAction(nameof(Index));
					}
				}
				if (currentUser.Email != userDto.Email)
				{
					var email = await _userManager.FindByEmailAsync(userDto.Email);
					if (email != null)
					{
						TempData["Message"] = $"{userDto.Email} is already taken.";
						return RedirectToAction(nameof(Index));
					}
				}
				await _serviceManager.AuthService.UpdateForUserAsync(userDto, currentUser.UserName, photo, deletePhoto);
				TempData["Message"] = "Changes updated";
				return RedirectToAction(nameof(Index));
			}
			TempData["Message"] = "Error";
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> LoginAsync(string returnUrl)
		{
			// returnUrl'i işleyin
			if (Url.IsLocalUrl(returnUrl))
			{
				var request = HttpContext.Request;
				var baseUrl = $"{request.Scheme}://{request.Host}";
				var uri = new Uri(baseUrl + returnUrl);
				var resultQuery = HttpUtility.ParseQueryString(uri.Query).Get("Result");
				TempData["MailCheckError"] = resultQuery;
			}
			await _signInManager.SignOutAsync();
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login([FromForm] LoginModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.Password == "Twitter" || model.Password == "Google" || model.Password == "Github")
				{
					ModelState.AddModelError(string.Empty, "You can't use this password.If you have previously logged in via social media, log in again with the social media button or reset your password.");
					return View();
				}
				else
				{
					CustomUser user = await _userManager.FindByNameAsync(model.Username);
					if (user is not null)
					{
						await _signInManager.SignOutAsync();
						if ((await _signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
						{
							if (user.LockoutEnabled == false)
							{
								await _signInManager.SignOutAsync();
								return RedirectToAction(nameof(ForgotPassword), user.UserName);
							}
							return RedirectToAction(nameof(Index));
						}
					}
					ModelState.AddModelError(string.Empty, "Username or password not found.");
				}
			}
			return View();
		}
		public async Task<IActionResult> SignUp()
		{
			await _signInManager.SignOutAsync();
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SignUp([FromForm] UserDtoForCreation userDto, string ConfirmPassword, IFormFile? photo)
		{
			if (userDto.Email != null)
			{
				bool IsMailValid = await VerifyEmail(userDto.Email);
				if (IsMailValid == true)
				{
					if (ModelState.IsValid)
					{
						if (userDto.Password == "Twitter" || userDto.Password == "Google" || userDto.Password == "Github")
						{
							ModelState.AddModelError(string.Empty, "You can't use this password.");
							return View(userDto);
						}
						else
						{
							var user = await _serviceManager.AuthService.GetOneUserAsync(userDto.UserName);
							if (user != null)
							{
								ModelState.AddModelError(string.Empty, $"{userDto.UserName} is already taken.");
								return View(userDto);
							}
							var userMail = await _userManager.FindByEmailAsync(userDto.Email);
							if (userMail != null)
							{
								ModelState.AddModelError(string.Empty, $"{userDto.Email} is already taken.");
								return View(userDto);
							}
						}
						if (userDto.Password == ConfirmPassword)
						{
							var result = await _serviceManager.AuthService.CreateUserAsync(userDto, photo);
							if (result.Succeeded)
							{
								return RedirectToAction(nameof(Index));
							}
							foreach (var error in result.Errors)
							{
								ModelState.AddModelError(string.Empty, error.Description);
							}
						}
						else
						{
							ModelState.AddModelError(string.Empty, "Password and Confirm password must be same.");
						}
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, "This email address is invalid");
					return View(userDto);
				}
			}
			return View(userDto);
		}
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return Redirect("/");
		}
		public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword, string userName)
		{
			if (userName is not null)
			{
				if (newPassword == confirmPassword)
				{
					var user = await _serviceManager.AuthService.GetOneUserAsync(userName);
					var passwordIsValid = await _userManager.CheckPasswordAsync(user, oldPassword);
					if (passwordIsValid == true)
					{
						var model = new ResetPasswordDto()
						{
							UserName = userName,
							OldPassword = oldPassword,
							Password = newPassword,
							ConfirmPassword = confirmPassword
						};
						await _serviceManager.AuthService.ResetPasswordAsync(model);
						TempData["Message"] = "Password changed.";
						return RedirectToAction(nameof(Index));
					}
					TempData["Message"] = "Old password is wrong.";
					return RedirectToAction(nameof(Index));
				}
				TempData["Message"] = "New password and confirm password must be same!";
				return RedirectToAction(nameof(Index));
			}
			TempData["Message"] = "Username is null.";
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> ForgotPassword(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			if (user == null)
			{
				TempData["ErrorMessage"] = "User not found";
				return RedirectToAction(nameof(Login));
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var callbackUrl = Url.Action("ResetPassword", "Home", new
			{
				token,
				email = user.Email,
			}, protocol: HttpContext.Request.Scheme);
			var content = EmailContent(callbackUrl);
			await _serviceManager.EmailSender.SendEmailAsync(user.Email, "Reset Password", content);

			await _signInManager.SignOutAsync();
			TempData["ErrorMessage"] = "Password reset link has been sent, please check your mailbox. If you are logging in for the first time, check your mailbox to determine your password.";
			return RedirectToAction(nameof(Login));
		}
		public string EmailContent(string callbackUrl)
		{
			return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            .email-container {{
                                font-family: Arial, sans-serif;
                                color: #333333;
                                padding: 20px;
                            }}
                            .email-header {{
                                text-align: center;
                            }}
                            .email-body {{
                                padding: 20px 0;
                            }}
                            .button {{
                                display: block;
                                width: 200px;
                                padding: 10px;
                                margin: 20px auto;
                                text-align: center;
                                background-color: #4CAF50;
                                color: white;
                                text-decoration: none;
                                border-radius: 5px;
                            }}
                            .email-footer {{
                                text-align: center;
                                font-size: 12px;
                                color: #777777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='email-container'>
                            <div class='email-header'>
                                <h2>Password Reset Request</h2>
                            </div>
                            <div class='email-body'>
                                <p>Hello,</p>
                                <p>To reset your password, please click on the following link:</p>
                                <a href='{callbackUrl}' class='button'>Reset Password</a>
                                <p>If you did not request this, please ignore this email and inform us.</p>
                            </div>
                            <div class='email-footer'>
                                <p>Have any questions? Reach us at: support@yourdomain.com</p>
                            </div>
                        </div>
                        </body>
                    </html>";
		}
		public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordModel model)
		{
			return View(model);
		}
		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordModel model, string? UrlTo = "/")
		{
			if (!ModelState.IsValid)
			{
				TempData["ErrorMessage"] = "Check again your password.";
				return View(model);
			}

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return RedirectToAction(nameof(SignIn));
			}

			var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
			if (result.Succeeded)
			{
				await _signInManager.SignOutAsync();
				TempData["ErrorMessage"] = "Your password has changed.";
				if (user.LockoutEnabled == false)
				{
					await _userManager.SetLockoutEnabledAsync(user, true);
				}
				return RedirectToAction(nameof(Login));
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return View(model);
		}
		public IActionResult SignInTwitter()
		{
			var authenticationProperties = new AuthenticationProperties { RedirectUri = "/" };
			return Challenge(authenticationProperties, "Twitter");
		}
		public IActionResult SignInGitHub()
		{
			var authenticationProperties = new AuthenticationProperties { RedirectUri = "/" };
			return Challenge(authenticationProperties, "GitHub");
		}
		public IActionResult SignInWithGoogle()
		{
			var authenticationProperties = new AuthenticationProperties
			{
				RedirectUri = "/"
			};
			return Challenge(authenticationProperties, "Google");
		}

	}
}