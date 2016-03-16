﻿using Lisa.Common.WebApi;
using Microsoft.AspNet.Mvc;
using System;
using System.Threading.Tasks;

namespace Lisa.Breakpoint.Api
{
    [Route("/reports/")]
    public class ReportController : Controller
    {
        public ReportController(Database database)
        {
            _db = database;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var reports = await _db.FetchReports();

            return new HttpOkObjectResult(reports);
        }

        [HttpGet("{id}", Name = "SingleReport")]
        public async Task<ActionResult> Get(Guid id)
        {
            object report = await _db.FetchReport(id);

            if (report == null)
            {
                return new HttpNotFoundResult();
            }

            return new HttpOkObjectResult(report);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] DynamicModel report)
        {
            if (report == null)
            {
                return new BadRequestResult();
            }

            var validationResult = _validator.Validate(report);
            if (validationResult.HasErrors)
            {
                return new UnprocessableEntityObjectResult(validationResult.Errors);
            }

            dynamic result = await _db.SaveReport(report);

            string location = Url.RouteUrl("SingleReport", new { id = result.id }, Request.Scheme);

            return new CreatedResult(location, result);
        }

        private Database _db;
        private Validator _validator = new ReportValidator();
    }
}