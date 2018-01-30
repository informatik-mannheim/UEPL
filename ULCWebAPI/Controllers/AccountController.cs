using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ULCWebAPI.Attributes;
using ULCWebAPI.Extensions;
using ULCWebAPI.Helper;
using ULCWebAPI.Models;
using ULCWebAPI.Security;

using System;
using System.Linq;
using System.Threading.Tasks;

using HttpStatusCode = System.Net.HttpStatusCode;

namespace ULCWebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]

#if !DEMO
    [TokenPermissionRequired]
#endif
    public class AccountController : Controller
    {
        private readonly APIDatabaseContext _context;
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authenticationService"></param>
        public AccountController(APIDatabaseContext context, IAuthenticationService authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        //[RequireHttps]
        [AllowAnonymous]
        [AllowWithoutToken]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var testUser = _authenticationService.Login(user.Name, user.Password);

                if (testUser != null)
                {
                    LoginToken loginToken = GenerateOrGetToken(testUser);
#if DEBUG
                    Tracer.TraceMessage($"== Token Information =={Environment.NewLine}{loginToken}");
#endif
                    return Json(loginToken);
                }    
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json("Login Failed!");
                }
            }
            catch(Exception e)
            {
                Tracer.TraceMessage($"Login Error: {e.Message}");
                throw e;
            }
        }

        private LoginToken GenerateOrGetToken(ApplicationUser user)
        {
            var tokens = _context.GetFullTable<LoginToken>();

            if (tokens.Any((lt) => lt.User.UserName == user.UserName))
            {
                var token = tokens.Where((lt) => lt.User.UserName == user.UserName).Single();

                if (token.Valid > DateTime.Now)
                    return token;
                else
                {
                    token.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    token.Valid = DateTime.Now.AddMinutes(30);
                    _context.SaveChanges();
                    return token;
                }
            }
            else
            {
                var token = new LoginToken() { Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()), User = user, Valid = DateTime.Now.AddMinutes(30) };
                _context.Tokens.Add(token);
                _context.SaveChanges();
                return token;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            var token = Request.GetToken();

            if (_context.Tokens.Any((lt) => lt.Token == token))
            {
                _context.Tokens.Remove(_context.Tokens.Where((lt) => lt.Token == token).First());
                _context.SaveChanges();
                return Content("Logout successful");
            }

            return BadRequest("No token present");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        public IActionResult GetUserInfo()
        {
            var token = Request.GetToken();

            if (!_context.IsTokenValid(token))
            {
                return new ContentResult() { StatusCode = (int)HttpStatusCode.Unauthorized, Content = "Token is not valid!" };
            }

            var loginToken = _context.GetLoginToken(token);

            if (loginToken == null)
                return NotFound("Token not found!");

            return Json(new { user = loginToken.User, valid = loginToken.Valid });


        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            var token = Request.GetToken();

            if (_context.IsTokenValid(token))
            {
                var info = _context.GetLoginToken(token);

                if (info == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json("Unauthorized");
                }

                return Json(new { User = info.User, username = info.User.UserName, valid = info.Valid });
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("Forbidden");
            }
        }
    }
}