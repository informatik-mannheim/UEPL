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
    [TokenPermissionRequired]
    public class AccountController : Controller
    {
        private readonly APIDatabaseContext _context;
        private readonly IAuthenticationService _authenticationService;
        private readonly int _tokenLifespanInMinutes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authenticationService"></param>
        public AccountController(APIDatabaseContext context, IAuthenticationService authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
#if DEMO
            _tokenLifespanInMinutes = 1440;
#else
            _tokenLifespanInMinutes = 30;
#endif
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
        public IActionResult Login([FromBody] User user)
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
            catch (Exception e)
            {
                Tracer.TraceMessage($"Login Error: {e.Message}");
                throw e;
            }
        }

        internal LoginToken GenerateOrGetToken(ApplicationUser user)
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
                    token.Valid = DateTime.Now.AddMinutes(_tokenLifespanInMinutes);
                    _context.SaveChanges();
                    return token;
                }
            }
            else
            {
                var token = new LoginToken() { Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()), User = user, Valid = DateTime.Now.AddMinutes(_tokenLifespanInMinutes) };
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
                return Ok("Logout successful");
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
            var loginToken = _context.GetLoginToken(Request.GetToken());

            if (loginToken == null)
                return NotFound("Token not found!");

            return Ok(new { user = loginToken.User, valid = loginToken.Valid });


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

                return Json(new {  info.User, username = info.User.UserName, valid = info.Valid });
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json("Forbidden");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("lecture")]
        public IActionResult GetAllLectures()
        {
            var token = Request.GetToken();
            var info = _context.GetLoginToken(token);
            var lectures = info.User.UserLectures;

            return Ok(lectures);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("lecture")]
        public IActionResult ConnectLecture([FromBody] string id)
        {
            var token = Request.GetToken();
            var info = _context.GetLoginToken(token);

            var lectures = _context.UserLectures.Where(ul => ul.UserID == info.User.ID);

            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound("id");

            if (lectures.Any(l => l.LectureID == id))
                return NoContent();

            _context.UserLectures.Add(new UserLecture() { LectureID = id, UserID = info.User.ID });
            _context.SaveChanges(); 

            return Ok(info.User);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("lecture")]
        public IActionResult DisconnectLecture([FromBody] string id)
        {
            var token = Request.GetToken();
            var info = _context.GetLoginToken(token);

            var lectures = _context.UserLectures.Where(ul => ul.UserID == info.User.ID);

            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound("id");

            if (!lectures.Any(l => l.LectureID == id))
                return NotFound("Not Connected");

            var lecture = lectures.Single(l => l.LectureID == id);
            _context.UserLectures.Remove(lecture);
            _context.SaveChanges();

            return Ok(info.User);
        }

    }
}