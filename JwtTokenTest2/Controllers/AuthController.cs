﻿using JwtTokenTest2.Data;
using JwtTokenTest2.DTOs;
using JwtTokenTest2.Helper;
using JwtTokenTest2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JwtTokenTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {        
        private readonly IConfiguration _config;
        private readonly AppDbContext _appDbContext;        

        public AuthController(IConfiguration config, AppDbContext appDbContext)
        {           
            _config = config;
            _appDbContext = appDbContext;           
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto newUser)
        {

            var (hashedPassword, passwordSalt) = PasswordHasher.HashPassword(newUser.Password);
            var user = new User { 
                UserName = newUser.UserName,
                PasswordHash = hashedPassword,
                PasswordSalt = passwordSalt
            };
            
            await _appDbContext.AddAsync(user);
            var usuarioCreado = _appDbContext.SaveChangesAsync().Result;

            if (usuarioCreado >= 0)
            {
                return CreatedAtAction("Register", user.Id, user);
            }
            return BadRequest();
        }

        
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto loginUser)
        {
            bool validPass = false;
            var user = _appDbContext.Users.FirstOrDefault(u => u.UserName.Equals(loginUser.UserName));
            if (user != null)
            {
                validPass = PasswordHasher.VerifyPassword(user.PasswordHash, user.PasswordSalt, loginUser.Password);
            }
            if (user == null || !validPass)
            {
                return Unauthorized("Credenciales inválidas");
            }
   
            var token = JwtTokenManager.GenerateJwtToken(user, _config);
            return Ok(new { Token = token });
        }



        /*
        [HttpPost("getNewAccessToken")]
        public IActionResult GetNewAccessToken()
        {
            var validRefreshToken = JwtTokenManager.ValidRefreshToken(_contextAccessor, _config);
            if (string.IsNullOrEmpty(validRefreshToken))
            {
                return BadRequest("Requiere nuevo inicio de sesión para obtener un nuevo Token de acceso.");
            }
            var userLogin = JwtTokenManager.GetUserFromRefreshToken(validRefreshToken, _config, _appDbContext);
            if (userLogin == null)
            {
                return Unauthorized("Credenciales inválidas");
            }
            var token = JwtTokenManager.GenerateJwtToken(userLogin, _config);
            return Ok(new { Token = token });
        }
        */
    }
}
