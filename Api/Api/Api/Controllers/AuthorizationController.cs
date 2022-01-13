﻿using Api.Model;
using Api.Services.AuthorizationServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Net;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationController : Controller
    {
        private readonly IAuthorizationService _authorizationservice;

        public AuthorizationController(IAuthorizationService authorizationservice)
        {
            _authorizationservice = authorizationservice; 
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInModel loginModel)
        {
            try
            {
                return Ok(await _authorizationservice.SignIn(loginModel)); 
            }
            catch(UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized); 
            }
        }
    }
}
