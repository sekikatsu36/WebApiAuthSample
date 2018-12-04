using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebApiAuthSample.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(int? id)
        {
            if(id == null)
            {
                // ID未指定だと400エラー
                return new CustomJsonResult(HttpStatusCode.BadRequest, new
                {
                    Type = this.GetType().FullName,
                    Title = "The Access code is expired or invalid.",
                    Instance = Request?.Path.Value
                });
            }

            var result = new
            {
                Id = id,
                Value = id.ToString()
            };

            //200で成功
            return new CustomJsonResult(HttpStatusCode.OK, result);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
