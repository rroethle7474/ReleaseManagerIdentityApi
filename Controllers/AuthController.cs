using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using ReleaseManagerIdentityApi.Services.Auth;

namespace ReleaseManagerIdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterUserAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while registering user" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while logging in" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while refreshing token" });
            }
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
        {
            try
            {
                await _authService.RevokeTokenAsync(request.RefreshToken);
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while revoking token" });
            }
        }
    }
}