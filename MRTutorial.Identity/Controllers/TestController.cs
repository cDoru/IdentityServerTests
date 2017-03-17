﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace MRTutorial.Identity.Controllers
{
    [Authorize]
    [RoutePrefix("test")]
    public class TestController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            var user = User as ClaimsPrincipal;
            return Json(user);
        }
    }
}
