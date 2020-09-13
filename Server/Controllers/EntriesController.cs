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
        public ActionResult<IEnumerable<Entry>> GetEntries(int year, int month)
        {
            // Validate year-month range
            try
            {
                new DateTime(year, month, 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest("Invalid year and month provided");
            }

            return Ok(Database.GetEntries(year, month));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)] // Successful update of existing entry
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Unsuccessful update of existing entry
        [ProducesResponseType(StatusCodes.Status201Created)] // Successful creation of new entry
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Unsuccessful creation of new entry
        public ActionResult<Entry> AddOrUpdateEntry(Entry newEntry, bool updateExisting = false)
        {
            if (updateExisting)
            {
                var updateResult = Database.UpdateEntry(newEntry);
                if (updateResult) return Ok();
                return BadRequest("Update failed, entry does not exist in database.");
            }

            else {
                var addResult = Database.AddEntry(newEntry);
                if (addResult == AddEntryResult.EntryExists)
                {
                    return Conflict($"Entry at {newEntry.Date} already exists.");
                }
                return CreatedAtAction(nameof(GetEntries), new { date = newEntry.Date }, newEntry);
            }            
        }

        private DateTime? TryParseDate(string dateString)
        {
            var parseResult = DateTime.TryParseExact(dateString, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var parsedDate);
            return parseResult ? parsedDate : (DateTime?)null;
        }
    }
}