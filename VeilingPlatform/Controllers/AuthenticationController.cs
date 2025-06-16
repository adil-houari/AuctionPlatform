using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VeilingPlatform.DTO.Authentication;

namespace VeilingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var userExists = await _userManager.FindByNameAsync(dto.Email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
                {
                    Status = "Error",
                    Message = "User already exists!"
                });
            }

            IdentityUser user = new IdentityUser
            {
                Email = dto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                });
            }

            if (!string.IsNullOrEmpty(dto.SubscriptionType))
            {
                await _userManager.AddClaimAsync(user, new Claim("SubscriptionType", dto.SubscriptionType));
            }

            return Ok(new ResponseDto { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var userClaims = await _userManager.GetClaimsAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                claims.AddRange(userClaims);

                var token = GetToken(claims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }


        private JwtSecurityToken GetToken(List<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }

    }
}





