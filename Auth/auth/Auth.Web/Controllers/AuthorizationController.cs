using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthorizationServer.Web.Domain;
using AuthorizationServer.Web.Dto;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreConstants;

namespace AuthorizationServer.Web.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthorizationController(
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        
        [HttpPost("~/connect/register")]
        public async Task<IActionResult> Create([FromForm] UserRigisterDto userDto)
        {
            Validator.UserRegisterDtoValidator(userDto);
            User user = new User(userDto.Email!, userDto.FirstName!, userDto.LastName!, userDto.PhoneNumber!);
            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request?.IsPasswordGrantType() == true)
            {
                var user = await _userManager.FindByNameAsync(request.Username);

                if (user == null)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [Properties.ErrorDescription] =
                            "The username/password couple is invalid."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                var result =
                    await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    var properties = new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [Properties.ErrorDescription] =
                            "The username/password couple is invalid."
                    });

                    return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                var key = Encoding.ASCII.GetBytes
                    ("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
                // Create a new ClaimsPrincipal containing the claims that
                // will be used to create an id_token, a token or a code.
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                        new Claim(ClaimTypes.Email, user.UserName),
                        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    Issuer = "http://auth:5200/",
                    Audience = "http://auth:5200/",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(tokenHandler.WriteToken(token));
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }
    }
}
