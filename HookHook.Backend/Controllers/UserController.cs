using HookHook.Backend.Entities;
using HookHook.Backend.Exceptions;
using HookHook.Backend.Models;
using HookHook.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HookHook.Backend.Controllers
{
    /// <summary>
    /// user information/auth route
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;

        public UserController(UserService service) =>
            _service = service;

        /// <summary>
        /// Register an user to database
        /// </summary>
        /// <param name="form">User informations</param>
        /// <returns>return newly created if succesfully registered</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult Create([FromBody] RegisterForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(form);
            var user = new User(form.Email);
            user.Username = form.Username;
            user.FirstName = form.FirstName;
            user.LastName = form.LastName;
            user.Password = form.Password;
            if (_service.GetUsers().Count == 0)
                user.Role = "Admin";
            try
            {
                _service.Register(user);
            }
            catch (UserException ex)
            {
                return ex.Type switch
                {
                    TypeUserException.Email => BadRequest(new {errors = new {Email = ex.Message}}),
                    TypeUserException.Password => BadRequest(new {errors = new {Password = ex.Message}}),
                    TypeUserException.Username => BadRequest(new {errors = new {Username = ex.Message}}),
                    _ => BadRequest(new {error = ex.Message})
                };
            }

            return Created("", null);
        }


        /// <summary>
        /// Register an user to database with OAuth
        /// </summary>
        /// <param name="form">User informations</param>
        /// <returns>return newly created if succesfully registered</returns>
        [HttpPost("oauth/{provider}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> OAuth(string provider, [BindRequired] [FromQuery] string code)
        {
            if (string.Equals(provider, "DiscordOAuth", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    string token = await _service.DiscordOAuth(code, HttpContext);

                    return Ok(new {token});
                }
                catch (ApiException ex)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new {error = ex.Message});
                }
            }

            if (string.Equals(provider, "GitHubOAuth", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    string token = await _service.GitHubOAuth(code, HttpContext);

                    return Ok(new {token});
                }
                catch (ApiException ex)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, new {error = ex.Message});
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// Login user to API
        /// </summary>
        /// <param name="form">User informations</param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Login([FromBody] LoginForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(form);
            try
            {
                var token = _service.Authenticate(form.Username, form.Password);
                return Ok(new { token });
            }
            catch (MongoException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (UserException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}