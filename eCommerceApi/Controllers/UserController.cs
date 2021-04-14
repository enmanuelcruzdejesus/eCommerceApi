using eCommerce.Model.Entities;
using eCommerceApi.Model;
using eCommerceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eCommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JWTSettings _jwtSettings;

        public UserController(IOptions<JWTSettings> options)
        {
            _jwtSettings = options.Value;
        }


        [HttpGet("Login")]
        public async Task<ActionResult<UserWithToken>> Login([FromBody] Users user)
        {
            var u =  UserSecurity.Login(user.username, user.password);
            var uToken = new UserWithToken(u);


            if (u == null)
                return NotFound();


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new System.Security.Claims.Claim[] 
                {
                      new System.Security.Claims.Claim(ClaimTypes.Name,user.username)
                }),
                Expires = DateTime.UtcNow.AddMonths(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            uToken.Token =  tokenHandler.WriteToken(token);
            return uToken;
        }
    }
}
