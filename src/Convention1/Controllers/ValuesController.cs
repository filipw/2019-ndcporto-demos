using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Convention1.Controllers
{
    public class ValuesController : ControllerBase
    {
        [HttpGet("values")]
        public ActionResult<IEnumerable<string>> GetValues()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("secured/values")]
        public ActionResult<IEnumerable<string>> AuthorizedGetValues()
        {
           return new string[] { "secure value1", "secure value2" };
        }
    }
}
