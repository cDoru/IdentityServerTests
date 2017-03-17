using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace MRTutorial.Identity.Client.Controllers
{
    [Route("identity")]
    [Authorize]
    public class IdentityController : ApiController
    {
        public IHttpActionResult Get()
        {
            var user = User as ClaimsPrincipal;
            if (user == null) 
                throw new InvalidOperationException();

            var claims = from c in user.Claims
                select new
                {
                    type = c.Type,
                    value = c.Value
                };

            return Json(claims);
        }
    }
}
