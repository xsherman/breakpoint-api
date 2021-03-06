﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lisa.Breakpoint.Api
{
    [Route("/users/")]
    public class UserController
    {
        public UserController (Database database)
        {
            _db = database;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] string project)
        {
            var filter = new List<Tuple<string, string>>();
            var users = new List<object>();
            if (project != null)
            {
                filter.Add(Tuple.Create("project", project));
            }

            var reports = await _db.FetchReports(filter);

            foreach (dynamic report in reports)
            {
                if (report.Assignee != null)
                {
                    if (!users.Contains(report.Assignee))
                    {
                        users.Add(report.Assignee);
                    }
                }
            }
            return new OkObjectResult(users);
        }

        private Database _db;
    }
}