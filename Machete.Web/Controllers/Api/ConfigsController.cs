﻿using AutoMapper;
using Machete.Domain;
using Machete.Service;
using Machete.Web.Helpers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machete.Web.Controllers.Api
{
    [Route("api/configs")]
    [ApiController]
    public class ConfigsController : MacheteApiController
    {
        private readonly IConfigService _serv;
        private readonly IMapper _map; 

        public ConfigsController(IConfigService serv, IMapper map)
        {
            this._serv = serv;
            this._map = map;
        }

        // GET: api/Configs
        //[Authorize(Roles = "Administrator")]
        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public ActionResult Get()
        {
            var result = _serv.GetMany(c => c.publicConfig == true);
            return new JsonResult(new { data = result });
        }

        // GET: api/Configs/5
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("{id}")]
        public JsonResult Get(int id)
        {
            var result = _serv.Get(id);
            return new JsonResult(new { data = result });
        }

        // POST: api/Configs
        [Authorize(Roles = "Administrator")]
        [HttpPost("")]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Configs/5
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Configs/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
