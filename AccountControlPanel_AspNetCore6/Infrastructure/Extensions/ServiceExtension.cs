using Business.Contracts;
using Business;
using DataAccess;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Entities.Dtos;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Security.Principal;

namespace AccountControlPanel_AspNetCore6.Infrastructure.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("sqlconnection"),
                    b => b.MigrationsAssembly("AccountControlPanel_AspNetCore6"));

                options.EnableSensitiveDataLogging(true);
            });
        }
        public static void ConfigureApplicationCookie(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Home/Login");
                options.LogoutPath = new PathString("/Home/Logout");
                options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1000); // Cookie süresi
                options.SlidingExpiration = true; // Süre yarıda yenilenirse cookie süresi resetlenir

                // Güvenlik ayarları
                options.Cookie.HttpOnly = true; // Cookie'lerin JavaScript tarafından erişilemez olmasını sağlar
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS üzerinden cookie gönderilmesini zorunlu kılar
                options.Cookie.SameSite = SameSiteMode.Lax; // CSRF saldırılarına karşı koruma sağlar

                // Cookie isimlendirmesi
                options.Cookie.Name = "AuthManagerAuthCookie";
            });
        }

		public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddAuthentication()
					.AddGoogle(options =>
					{
						configuration.Bind("Authentication:Google", options);
						options.ClientId = configuration["Authentication:Google:ClientId"];
						options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
						options.CallbackPath = new PathString("/signin-google");

						options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
						options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");

						options.Events = new OAuthEvents
						{
							OnCreatingTicket = async context =>
							{
								var principal = context.Principal;
								var accessToken = context.AccessToken;
								var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
								request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

								var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
								response.EnsureSuccessStatusCode();

								var userGoogle = JObject.Parse(await response.Content.ReadAsStringAsync());

								var googleId = userGoogle.Value<string>("id");
								var name = userGoogle.Value<string>("name");
								var email = userGoogle.Value<string>("email");
								var usernameFromMail = email.Split('@')[0];
								var services = context.HttpContext.RequestServices;
								var getNormalizeUserName = services.GetService<NormalizeStringService>();
								var userName = getNormalizeUserName.NormalizeString(name);
								userName = userName + usernameFromMail;
								var model = new UserDtoForCreation()
								{
									UserName = userName,
									FirstName = name,
									Email = email,
									Password = "Google",
								};
								var result = await SocialMediaAuthAsync(model, userName, "Google", googleId, services);
								if (result.Errors.Count() > 0)
								{
									var identity = principal.Identity as ClaimsIdentity;
									identity.AddClaim(new Claim("ResultAuth", result.Errors.FirstOrDefault().Description));
									var customValue = principal.FindFirst("ResultAuth")?.Value;
									if (customValue == "First.")
									{
										var usernameforPasswordReset = userName + "-" + "Google";
										identity.AddClaim(new Claim("Username", usernameforPasswordReset));
									}
								}
							},
							OnTicketReceived = context =>
							{
								var principal = context.Principal;
								var customValue = principal.FindFirst("ResultAuth")?.Value;
								var userName = principal.FindFirst("Username")?.Value;
								if (customValue == "First.")
								{
									context.ReturnUri = $"/Home/ForgotPassword?username={userName}";
								}
								else
								{
									context.ReturnUri = $"/?Result={customValue}";
								}
								context.Response.Redirect(context.ReturnUri);
								context.HandleResponse();

								return Task.CompletedTask;
							}
						};
					})
					.AddTwitter(options =>
					{
						configuration.Bind("Authentication:Twitter", options);
						options.ConsumerKey = configuration["Authentication:Twitter:ConsumerAPIKey"];
						options.ConsumerSecret = configuration["Authentication:Twitter:ConsumerSecret"];
						options.CallbackPath = new PathString("/signin-twitter");
						options.RetrieveUserDetails = true;
						options.Events = new TwitterEvents
						{
							OnCreatingTicket = async context =>
							{
								var principal = context.Principal;
								var twitterId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
								var username = context.Principal.FindFirstValue(ClaimTypes.Name);
								var name = principal.FindFirstValue(ClaimTypes.Name);
								var email = principal.FindFirstValue(ClaimTypes.Email);
								var phone = principal.FindFirstValue(ClaimTypes.HomePhone);
								var dateOfBirthStr = principal.FindFirstValue(ClaimTypes.DateOfBirth);
								var url = principal.FindFirstValue("urn:twitter:profileurl");

								DateTime dateOfBirth;
								DateTime.TryParse(dateOfBirthStr, out dateOfBirth);
								var services = context.HttpContext.RequestServices;
								var model = new UserDtoForCreation()
								{
									UserName = username,
									FirstName = name,
									UserPhotoUrl = url,
									PhoneNumber = phone,
									Password = "Twitter",
									Email = email,
									DateOfBirth = dateOfBirth
								};
								var result = await SocialMediaAuthAsync(model, name, "Twitter", twitterId, services);
								if (result.Errors.Count() > 0)
								{
									var identity = principal.Identity as ClaimsIdentity;
									identity.AddClaim(new Claim("ResultAuth", result.Errors.FirstOrDefault().Description));
									var customValue = principal.FindFirst("ResultAuth")?.Value;
									if (customValue == "First.")
									{
										var usernameforPasswordReset = username + "-" + "Twitter";
										identity.AddClaim(new Claim("Username", usernameforPasswordReset));
									}
								}
							},
							OnTicketReceived = context =>
							{
								var principal = context.Principal;
								var customValue = principal.FindFirst("ResultAuth")?.Value;
								var userName = principal.FindFirst("Username")?.Value;
								if (customValue == "First.")
								{
									context.ReturnUri = $"/Home/ForgotPassword?username={userName}";
								}
								else
								{
									context.ReturnUri = $"/?Result={customValue}";
								}
								context.Response.Redirect(context.ReturnUri);
								context.HandleResponse();

								return Task.CompletedTask;
							}
						};
					})
					.AddGitHub(options =>
					{
						configuration.Bind("Authentication:GitHub", options);
						options.ClientId = configuration["Authentication:GitHub:ClientId"];
						options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"];
						options.CallbackPath = new PathString("/signin-github");
						options.Scope.Add("user:email"); // GitHub'da e-posta erişimi için gerekli

						options.Events = new OAuthEvents
						{
							OnCreatingTicket = async context =>
							{
								var principal = context.Principal;
								var accessToken = context.AccessToken;
								var githubId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
								var name = context.Principal.FindFirstValue(ClaimTypes.Name);
								var email = context.Principal.FindFirstValue(ClaimTypes.Email);
								var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
								request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

								var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
								response.EnsureSuccessStatusCode();

								var userGit = JObject.Parse(await response.Content.ReadAsStringAsync());

								var avatarUrl = userGit.Value<string>("avatar_url");
								var services = context.HttpContext.RequestServices;
								var model = new UserDtoForCreation()
								{
									UserName = name,
									FirstName = name,
									Email = email,
									UserPhotoUrl = avatarUrl,
									Password = "Github",
								};
								var result = await SocialMediaAuthAsync(model, name, "Github", githubId, services);
								if (result.Errors.Count() > 0)
								{
									var identity = principal.Identity as ClaimsIdentity;
									identity.AddClaim(new Claim("ResultAuth", result.Errors.FirstOrDefault().Description));
									var customValue = principal.FindFirst("ResultAuth")?.Value;
									if (customValue == "First.")
									{
										var usernameforPasswordReset = name + "-" + "Github";
										identity.AddClaim(new Claim("Username", usernameforPasswordReset));
									}
								}
							},
							OnTicketReceived = context =>
							{
								var principal = context.Principal;
								var customValue = principal.FindFirst("ResultAuth")?.Value;
								var userName = principal.FindFirst("Username")?.Value;
								if (customValue == "First.")
								{
									context.ReturnUri = $"/Home/ForgotPassword?username={userName}";
								}
								else
								{
									context.ReturnUri = $"/?Result={customValue}";
								}
								context.Response.Redirect(context.ReturnUri);
								context.HandleResponse();

								return Task.CompletedTask;
							}
						};
					});

		}
		public static async Task<IdentityResult> SocialMediaAuthAsync(UserDtoForCreation userDtoFor, string username, string SocialMedia, string SocialMediaUserId, IServiceProvider? service)
		{
			var authManager = service.GetService<IAuthService>();
			if (authManager != null)
			{
				var user = await authManager.GetOneUserAsync(username);
				if (user == null)
				{
					var Result = await authManager.CheckUserLoginAsync(SocialMedia, SocialMediaUserId, userDtoFor);
					if (Result == "First.")
					{
						var errorFirst = new IdentityError { Description = $"{Result}" };
						var errorResultFirst = IdentityResult.Failed(errorFirst);
						return errorResultFirst;
					}
					else if (Result != "Ok.")
					{
						var result = await authManager.CreateUserAsync(userDtoFor, null);
						if (result.Succeeded)
						{
							user = await authManager.GetOneUserAsync(username);
							var loginInfo = new UserLoginInfo(SocialMedia, SocialMediaUserId, SocialMedia);
							await authManager.AddUserLoginAsync(user, loginInfo);
						}
						return result;
					}
					var error = new IdentityError { Description = $"{Result}" };
					var errorResult = IdentityResult.Failed(error);
					return errorResult;

				}
				else
				{
					var Result = await authManager.CheckUserLoginAsync(SocialMedia, SocialMediaUserId, userDtoFor);
					var error = new IdentityError { Description = $"{Result}" };
					var errorResult = IdentityResult.Failed(error);
					return errorResult;
				}
			}
			var errorManager = new IdentityError { Description = $"Manager is null" };
			var errorResultManager = IdentityResult.Failed(errorManager);
			return errorResultManager;
		}
		public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<CustomUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<RepositoryContext>()
                .AddDefaultTokenProviders();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                //options.AddPolicy("EditorOnly", policy => policy.RequireRole("Editor"));
            });

        }
        public static void ConfigureServiceRegistration(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<PhotoInsertService>();
            services.AddScoped<NormalizeStringService>();
            services.AddTransient<IEmailSender, EmailSenderService>();
            services.AddHttpContextAccessor();

        }
        public static void ConfigureSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "AccountControlPanel_AspNetCore6.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
        public static void ConfigureMailSenderServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddTransient<IEmailSender, EmailSenderService>();
        }
        public static void ConfigureRouting(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = true;
            });
        }
    }
}
