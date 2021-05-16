using BlogLap.Models;
using BlogLap.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLap.Web.Controller
{
    //http://localhost
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenServices _tokenServices;
        private readonly UserManager<ApplicationUserIdentity> _userManager;
        private readonly SignInManager<ApplicationUserIdentity> _signInManager;

        public AccountController(ITokenServices tokenServices, UserManager<ApplicationUserIdentity> userManager,
            SignInManager<ApplicationUserIdentity> signInManager)
        {
            _tokenServices = tokenServices;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> Register(ApplicatonUserCreate applicatonUserCreate)
        {
            var applicationUserIdentity = new ApplicationUserIdentity
            {
                Username = applicatonUserCreate.Username,
                Email = applicatonUserCreate.Email,
                FullName = applicatonUserCreate.FullName
            };

            var result = await _userManager.CreateAsync(applicationUserIdentity, applicatonUserCreate.Password);

            if (result.Succeeded)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    ApplicationUsrId = applicationUserIdentity.ApplicationUserId,
                    Username = applicationUserIdentity.Username,
                    Email = applicationUserIdentity.Email,
                    Fullname = applicatonUserCreate.FullName,
                    Token = _tokenServices.CreateToken(applicationUserIdentity)

                };

                return user;
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> Login(ApplicationUserLogin applicationUserLogin)
        {
            var applicationUserIdentity = await _userManager.FindByNameAsync(applicationUserLogin.Username);
            if (applicationUserIdentity != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(applicationUserIdentity, applicationUserLogin.Password, false);

                if (result.Succeeded)
                {
                    ApplicationUser applicationUser = new ApplicationUser()
                    {
                        ApplicationUsrId = applicationUserIdentity.ApplicationUserId,
                        Username = applicationUserIdentity.Username,
                        Email = applicationUserIdentity.Email,
                        Fullname = applicationUserIdentity.FullName,
                        Token = _tokenServices.CreateToken(applicationUserIdentity)

                    };
                    return Ok(applicationUser);

                }
            }

            return BadRequest("Invalid login attempt.");

        }

    }
}
