﻿using AuthServer.API.Models;
using AuthServer.API.Models.Requests;
using AuthServer.API.Models.Responses;
using AuthServer.API.Services.PasswordHashes;
using AuthServer.API.Services.TokenGenerators;
using AuthServer.API.Services.UserRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AccessTokenGenerator _accessTokenGenerator;

        public AuthenticationController(IUserRepository userRepository, IPasswordHasher passwordHasher, AccessTokenGenerator accessTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            if (registerRequest.Password != registerRequest.ConfirmPassword)
            {
                return BadRequest(new ErrorResponse("Password does not match confirm password"));
            }

            User existingUserByEmail = await _userRepository.GetByEmail(registerRequest.Email);
            if (existingUserByEmail != null)
            {
                return Conflict(new ErrorResponse("Email already exist"));
            }

            User existingUserByUsername = await _userRepository.GetByUsername(registerRequest.Username);
            if (existingUserByUsername != null)
            {
                return Conflict(new ErrorResponse("Username already exist"));
            }

            string passwordHash = _passwordHasher.HashPassword(registerRequest.Password);

            User registrationUser = new User()
            {
                Email = registerRequest.Email,
                Username = registerRequest.Username,
                PasswordHash = passwordHash
            };

            await _userRepository.Create(registrationUser);

            return Ok();
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            User user = await _userRepository.GetByUsername(loginRequest.Username);
            if (user == null)
            {
                return Unauthorized();
            }
            
            bool isCorrectPassword = 
                _passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash);
            if (!isCorrectPassword)
            {
                return Unauthorized();
            }

            string accessToken = _accessTokenGenerator.GenerateToken(user);

            return Ok(new AuthenticatedUserResponse() 
            {
                AccessToken = accessToken
            });
        }

        private IActionResult BadRequestModelState()
        {
            IEnumerable<string> errorMessages =
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));

            return BadRequest(new ErrorResponse(errorMessages));
        }
    }
}
