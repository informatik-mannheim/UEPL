using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using ULCWebAPI.Attributes;
using ULCWebAPI.Extensions;
using ULCWebAPI.Helper;
using ULCWebAPI.Models;

using IOF = System.IO.File;

using static ULCWebAPI.Helper.FileHelper;

namespace ULCWebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [TokenPermissionRequired]
    [AllowWithoutToken(new string[] { "GET" })]
    public class LectureController : Controller
    {
        private readonly APIDatabaseContext _context;
        private readonly IHostingEnvironment _environment;
        private readonly string _storagePath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="environment"></param>
        public LectureController(APIDatabaseContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _storagePath = Path.Combine(_environment.ContentRootPath, "storage", "lectures");
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

            var lectureItem = await _context.GetFullTable<Lecture>().SingleOrDefaultAsync(m => m.ID == id);

            if (lectureItem == null)
                return NotFound(id);

            var data = new LectureResponse { ID = lectureItem.ID, Name = lectureItem.Name, Content = new List<Package>() };

            if (lectureItem.Contents != null)
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
            else if (!lectureExists(id))
                return NotFound(id);


            var item = _context.Lectures.First(li => li.ID == id);

            try
            {
                item.Name = (lecture.Name != null && lecture.Name != item.Name) ? lecture.Name : item.Name;
                item.Version++;

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
            else if (lectureExists(lectureItem.ID))
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

            var lectureItem = await _context.GetFullTable<Lecture>().SingleOrDefaultAsync(m => m.ID == id);

            if (lectureItem == null)
                return NotFound(id);

            // alle contents element aus der Auflistung entfernen und danach löschen (FOREIGN KEY CONSTRAINT nachschauen)

            _context.Lectures.Remove(lectureItem);
            await _context.SaveChangesAsync();

            return Ok(lectureItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignPackages([FromRoute] string id, [FromBody] string[] data)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lecture = await _context.GetFullTable<Lecture>().SingleOrDefaultAsync(lect => lect.ID == id);

            if (lecture == null)
                return NotFound($"Lecture with id {id} not found!");
            
            lecture.Contents.Clear();

            foreach (var pkgID in data)
            {
                if (int.TryParse(pkgID, out int numericPackageID))
                {
                    var package = await _context.GetFullTable<Package>().SingleOrDefaultAsync(pack => pack.ID == numericPackageID);

                    if (package != null)
                        lecture.Contents.Add(package);
                }
            }

            lecture.Version++;

            await _context.SaveChangesAsync();

            return AcceptedAtAction(nameof(GetLecture), new { id = lecture.ID });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/file")]
        public IActionResult ListFiles([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!lectureExists(id))
                return NotFound(id);

            var lecture = _context.GetFullTable<Lecture>().Where(li => li.ID == id).Single();
            
            foreach(var item in lecture.StorageItems)
            {
                item.Url = this.FullURL(nameof(DownloadFile), new { id, filename = item.Filename });
            }

            return Ok(lecture.StorageItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpGet("{id}/file/{filename}")]
        public IActionResult DownloadFile([FromRoute] string id, [FromRoute] string filename)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!lectureExists(id))
                return NotFound(id);

            var file = _context.GetFullTable<Lecture>().Single(li => li.ID == id)
                       .StorageItems?.SingleOrDefault(lsi => lsi.Filename == filename);

            if (file == null)
                return NotFound(filename);
            else
            {
                var filePath = Path.Combine(_storagePath, file.LectureRef.ID, file.Filename);

                if (!IOF.Exists(filePath))
                    return NotFound("File does not exist on server!");
                else
                    return File(IOF.ReadAllBytes(filePath), "application/octet-stream");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DisableRequestSizeLimit]
        [HttpPost("{id}/file")]
        public IActionResult UploadFile([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!lectureExists(id))
                return NotFound();

            long size = 0;

            try
            {
                var lecture = _context.Lectures.Single((li) => li.ID == id);
                string dirPath = Path.Combine(_storagePath, lecture.ID);

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                var files = Request.Form.Files;
                var sha2 = System.Security.Cryptography.SHA256.Create();

                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    filename = Path.Combine(dirPath, filename);
                    var pathFileName = Path.GetFileName(filename);

                    size += file.Length;

                    var hashValue = "";
                    var isFilePresent = IOF.Exists(filename);
                    var isFileInStorage = lecture.StorageItems.Any(lsi => lsi.Filename == pathFileName);

                    if (!isFilePresent || !isFileInStorage)
                    {
                        if(!isFilePresent)
                        {
                            using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }
                        }

                        hashValue = ComputeHash(sha2, filename);

                        _context.LectureStorage.Add(new LectureStorageItem() { LectureRef = lecture, Filename = Path.GetFileName(filename), Hash = hashValue });
                        _context.SaveChanges();
                    }
                }

                _context.Lectures.Update(lecture);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Tracer.TraceMessage(e);
                throw;
            }

            return Ok(size);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpDelete("{id}/file/{filename}")]
        public async Task<IActionResult> DeleteUploadedFile([FromRoute] string id, [FromRoute] string filename)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!lectureExists(id))
                return NotFound(id);

            var file = _context.GetFullTable<Lecture>().Single(li => li.ID == id)
                          .StorageItems.SingleOrDefault(lsi => lsi.Filename == filename);

            if (file == null)
                return NotFound(filename);
            else
            {
                var filePath = Path.Combine(_storagePath, file.LectureRef.ID, file.Filename);

                if (!IOF.Exists(filePath))
                    return NotFound("File does not exist on server!");
                else
                {
                    // Remove File
                    IOF.Delete(filePath);
                    file.LectureRef.StorageItems?.Remove(file);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
        }

        private bool lectureExists(string id)
        {
            return _context.Lectures.Any(e => e.ID == id);
        }
    }
}