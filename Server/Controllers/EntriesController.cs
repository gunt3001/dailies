using System;
using System.Collections.Generic;
using System.Globalization;
using dailies.Server.Models;
using dailies.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dailies.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntriesController : ControllerBase
    {
        private IDatabase Database { get; }

        public EntriesController(IDatabase database)
        {
            Database = database;
        }

        [HttpGet]
        [Route("{date}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Entry> GetEntry(string date)
        {
            var parsedDate = TryParseDate(date);
            if (parsedDate == null)
            {
                return BadRequest();
            }

            var entry = Database.GetEntry(parsedDate.Value);
            if (entry == null) return NotFound();

            return Ok(entry);
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<Entry>> GetEntries(string start, string end)
        {
            var startDate = TryParseDate(start);
            var endDate = TryParseDate(end);

            if (startDate == null || endDate == null)
            {
                return BadRequest();
            }
            return Ok(Database.GetEntries(startDate.Value, endDate.Value));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<Entry> AddEntry(Entry newEntry)
        {
            if (Database.AddEntry(newEntry) == AddEntryResult.EntryExists)
            {
                return Conflict($"Entry at {newEntry.Date} already exists.");
            }

            return CreatedAtAction(nameof(GetEntries), new { date = newEntry.Date }, newEntry);
        }

        private DateTime? TryParseDate(string dateString)
        {
            var parseResult = DateTime.TryParseExact(dateString, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var parsedDate);
            return parseResult ? parsedDate : (DateTime?)null;
        }
    }
}