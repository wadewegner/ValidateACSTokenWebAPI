using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    public class BeerController : ApiController
    {
        // GET api/beer
        public IEnumerable<string> Get()
        {
            return new string[] { "Pliny the Younger", "Arrogant Bastard" };
        }

        // GET api/beer/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/beer
        public void Post(string value)
        {
        }

        // PUT api/beer/5
        public void Put(int id, string value)
        {
        }

        // DELETE api/beer/5
        public void Delete(int id)
        {
        }
    }
}
