using AuthServer.API.Models;
using AuthServer.API.Models.Requests;
using AuthServer.API.Models.Responses;
using AuthServer.API.Services.Authenticators;
using AuthServer.API.Services.PasswordHashes;
using AuthServer.API.Services.RefreshTokenRepositories;
using AuthServer.API.Services.TokenGenerators;
using AuthServer.API.Services.TokenValidators;
using AuthServer.API.Services.UserRepositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly Authenticator _authenticator;
        private readonly RefreshTokenValidator _refreshTokenValidator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthenticationController(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            RefreshTokenValidator refreshTokenValidator,
            IRefreshTokenRepository refreshTokenRepository, 
            Authenticator authenticator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _refreshTokenValidator = refreshTokenValidator;
            _refreshTokenRepository = refreshTokenRepository;
            _authenticator = authenticator;
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

            return Created(
                new Uri("/register", UriKind.RelativeOrAbsolute), 
                new { Email = registrationUser.Email, Name = registrationUser.Username 
                });
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

            AuthenticatedUserResponse response = await _authenticator.Authenticate(user);

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            bool isValidRefreshToken = _refreshTokenValidator.Validate(refreshRequest.RefreshToken);
            if (!isValidRefreshToken)
            {
                return BadRequest(new ErrorResponse("Invalid refresh token."));
            }

            RefreshToken refreshTokenDTO = await _refreshTokenRepository.GetByToken(refreshRequest.RefreshToken);
            if (refreshTokenDTO == null)
            {
                return BadRequest(new ErrorResponse("Refresh token not found."));
            }

            await _refreshTokenRepository.Delete(refreshTokenDTO.Id);

            User user = await _userRepository.GetById(refreshTokenDTO.UserId);
            if (user == null)
            {
                return BadRequest(new ErrorResponse("User not found."));
            }

            AuthenticatedUserResponse response = await _authenticator.Authenticate(user);

            return Ok(response);
        }

        private IActionResult BadRequestModelState()
        {
            IEnumerable<string> errorMessages =
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));

            return BadRequest(new ErrorResponse(errorMessages));
        }
    }
}
