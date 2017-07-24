using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Models;
using Microsoft.AspNetCore.Routing;
using ProjectAPI.Extensions;
using System.ComponentModel.DataAnnotations;
using System;
using ProjectAPI.Helper;

namespace ProjectAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class LectureController : Controller
    {
        private readonly APIDatabaseContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public LectureController(APIDatabaseContext context)
        {
            _context = context;

        }

        // GET: api/Lecture
        /// <summary>
        /// Returns a list of all available lectures
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">everything went well ;)</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<LectureReferenceResponse>), 200)]
        public IActionResult GetLectures()
        {
            return Ok(_context.Lectures.Select(li => new LectureReferenceResponse { ID = li.ID, Name = li.Name, Url = this.FullURL(nameof(GetLecture), new { id = li.ID }) }));
        }

        // GET: api/Lecture/5
        /// <summary>
        /// Get a specific Lecture item.
        /// </summary>
        /// <remarks>
        /// The id param is a string containing the short representation of the lecture.
        /// 
        ///     POST /api/lecture
        ///     {
        ///         "id": "TPE",
        ///         "name": "Techniken der Programmentwicklung",
        ///         "contents": null
        ///     }
        ///     
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>specific lecture item with the supplied id</returns>
        /// <response code="200"></response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Item with id not found!</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LectureResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> GetLecture([FromRoute, Required] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lectureItem = await _context.Lectures.Include(l => l.Contents).SingleOrDefaultAsync(m => m.ID == id);

            if (lectureItem == null)
                return NotFound(id);

            var data = new LectureResponse { ID = lectureItem.ID, Name = lectureItem.Name, Content = new List<Package>() };

            if(lectureItem.Contents != null)
            {
                foreach (var li in lectureItem.Contents)
                {
                    data.Content.Add(li);
                }
            }

            return Ok(data);
        }

        // PUT: api/Lecture/5
        /// <summary>
        /// Change values of an existing item
        /// </summary>
        /// <remarks>
        /// Only add fields to the lecture object if you want to change them.
        /// There is no need in adding the id or unchanged values to the object.
        /// </remarks>
        /// <param name="id">string based identifier (e.g. "TPE")</param>
        /// <param name="lecture">a lecture object containing the changes</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Lecture), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(string), 404)]
        public IActionResult PutLecture([FromRoute] string id, [FromBody] Lecture lecture)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!LectureItemExists(id))
                return NotFound(id);


            var item = _context.Lectures.First(li => li.ID == id);

            try
            {
                item.Name = (lecture.Name != null && lecture.Name != item.Name) ? lecture.Name : item.Name;

                _context.Update(item);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException dbce)
            {
                Tracer.TraceMessage(dbce.Message);
                return StatusCode(500);
            }

            return Ok(item);
        }

        /// <summary>
        /// Creates a new lecture or retrieves an already existing lecture.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="lectureItem">representation of a lecture item</param>
        /// <returns>Created or existing item</returns>
        /// <response code="200">Item with key already exists!</response>
        /// <response code="201">Sucessfully created new item!</response>
        /// <response code="400">Item not found!</response>
        // POST: api/Lecture
        [HttpPost]
        [ProducesResponseType(typeof(LectureResponse), 200)]
        [ProducesResponseType(typeof(LectureResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PostLectureItem([FromBody] Lecture lectureItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (LectureItemExists(lectureItem.ID))
                return await GetLecture(lectureItem.ID);

            _context.Lectures.Add(lectureItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLecture), new { id = lectureItem.ID }, lectureItem);
        }

        // DELETE: api/Lecture/5
        /// <summary>
        /// Deletes a specific lecture
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> DeleteLectureItem([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lectureItem = await _context.Lectures.SingleOrDefaultAsync(m => m.ID == id);

            if (lectureItem == null)
                return NotFound(id);

            _context.Lectures.Remove(lectureItem);
            await _context.SaveChangesAsync();

            return Ok(lectureItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pkgID"></param>
        /// <returns></returns>
        [HttpGet("{id}/attach/{pkgID}")]
        public async Task<IActionResult> AttachPackage([FromRoute] string id, [FromRoute] int pkgID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lecture = await _context.Lectures.SingleOrDefaultAsync(lect => lect.ID == id);

            if (lecture == null)
                return NotFound();

            var package = await _context.Packages.SingleOrDefaultAsync(pkg => pkg.ID == pkgID);

            if (package == null)
                return NotFound();

            if (lecture.Contents.Any(p => p.ID == package.ID))
                return Ok();

            try
            {
                lecture.Contents.Add(package);
                _context.Lectures.Update(lecture);
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e.Message);
                return StatusCode(500);
            }

            return AcceptedAtAction(nameof(GetLecture), new { id = lecture.ID });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pkgID"></param>
        /// <returns></returns>
        [HttpGet("{id}/detach/{pkgID}")]
        public async Task<IActionResult> DetachPackage([FromRoute] string id, [FromRoute] int pkgID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lecture = await _context.Lectures.Include(l => l.Contents).SingleOrDefaultAsync(lect => lect.ID == id);

            if (lecture == null)
                return NotFound();

            if (!lecture.Contents.Any(p => p.ID == pkgID))
                return Ok();

            try
            {
                lecture.Contents.Remove(lecture.Contents.First(p => p.ID == pkgID));
                _context.Lectures.Update(lecture);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Tracer.TraceMessage(e.Message);
                return StatusCode(500);
            }

            return AcceptedAtAction(nameof(GetLecture), new { id = lecture.ID });
        }

        private bool LectureItemExists(string id)
        {
            return _context.Lectures.Any(e => e.ID == id);
        }
    }
}