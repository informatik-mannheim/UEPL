using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using ULCWebAPI.Attributes;
using ULCWebAPI.Extensions;
using ULCWebAPI.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOF = System.IO.File;

namespace ULCWebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]

#if !DEMO
    [TokenPermissionRequired]
    [AllowWithoutToken(new string[] { "GET" })]
#endif
    public class ArtifactController : Controller
    {
        private readonly APIDatabaseContext _context;
        private readonly IHostingEnvironment _environment;
        private readonly FormOptions _defaultFormOptions;
        private readonly string _storagePath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="environment"></param>
        public ArtifactController(APIDatabaseContext context, IHostingEnvironment environment, IOptions<FormOptions> formOptions)
        {
            _defaultFormOptions = formOptions.Value;
            _context = context;
            _environment = environment;
            _storagePath = Path.Combine(_environment.ContentRootPath, "storage");
        }

        // GET: api/Artifacts
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowWithoutToken]
        public IActionResult GetArtifacts()
        {
            return Ok(_context.Artifacts.Select((art) => new { id = art.ID, url = this.FullURL(nameof(GetArtifact), new { id = art.ID }) }));
        }

        // GET: api/Artifacts/eclipse-x64-mars
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtifact([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var artifact = await _context.Artifacts.Include(a => a.StorageItems).SingleOrDefaultAsync(m => m.ID == id);

            if (artifact == null)
                return NotFound();

            ArtifactResponse ar = GenerateArtifactResponse(id, artifact);

            return Ok(ar);
        }

        internal ArtifactResponse GenerateArtifactResponse(string id, Artifact artifact)
        {
            ArtifactResponse ar = new ArtifactResponse
            {
                ID = id,
                InstallAction = artifact.InstallAction,
                RemoveAction = artifact.RemoveAction,
                SwitchAction = artifact.SwitchAction,
                UnswitchAction = artifact.UnswitchAction,
                StorageItems = new List<ArtifactStorageResponse>()
            };

            artifact.StorageItems?.ForEach((item) =>
            {
                ar.StorageItems.Add(new ArtifactStorageResponse
                {
                    FileName = item.Filename,
                    Url = this.FullURL(nameof(DownloadArtifact), new { id = id, filename = item.Filename })
                });
            });

            return ar;
        }

        // PUT: api/artifact
        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifact"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> PutArtifact([FromBody] Artifact artifact)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!ArtifactExists(artifact.ID))
                return NotFound($"Artifact with id {artifact.ID} not found!");

            var toChange = _context.Artifacts.Single(ar => ar.ID == artifact.ID);

            try
            {
                toChange.InstallAction = string.IsNullOrWhiteSpace(artifact.InstallAction) ? toChange.InstallAction : artifact.InstallAction;
                toChange.RemoveAction = string.IsNullOrWhiteSpace(artifact.RemoveAction) ? toChange.RemoveAction : artifact.RemoveAction;
                toChange.SwitchAction = string.IsNullOrWhiteSpace(artifact.SwitchAction) ? toChange.SwitchAction : artifact.SwitchAction;
                toChange.UnswitchAction = string.IsNullOrWhiteSpace(artifact.UnswitchAction) ? toChange.UnswitchAction : artifact.UnswitchAction;

                _context.Artifacts.Update(toChange);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok();
        }

        // POST: api/Artifacts
        /// <summary>
        /// 
        /// </summary>
        /// <param name="artifact"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostArtifact([FromBody] Artifact artifact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Artifacts.Add(artifact);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArtifact", new { id = artifact.ID }, artifact);
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
            else if (!ArtifactExists(id))
                return NotFound(id);

            var artifact = _context.Artifacts.Include(asi => asi.StorageItems).Where(ar => ar.ID == id).Single();
            var storageItems = artifact.StorageItems ?? new List<ArtifactStorageItem>();

            var data = new { Artifact = artifact.ID, Files = new List<dynamic>(), _self = this.FullURL(nameof(ListFiles), new { id = id }) };

            foreach(var item in storageItems)
            {
                data.Files.Add(new
                {
                    Filename = item.Filename,
                    Url = this.FullURL(nameof(DownloadArtifact), new { id = id, filename = item.Filename })
                });
            }

            return Ok(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpGet("{id}/file/{filename}")]
        public IActionResult DownloadArtifact([FromRoute] string id, [FromRoute] string filename)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!ArtifactExists(id))
                return NotFound(id);

            var artifact = _context.Artifacts.Include(asi => asi.StorageItems).Single(ar => ar.ID == id);

            if (artifact.StorageItems?.Count == 0)
                return NotFound(filename);

            var file = artifact.StorageItems?.SingleOrDefault(asi => asi.Filename == filename);

            if (file == null)
                return NotFound(filename);
            else
            {
                var filePath = Path.Combine(_storagePath, artifact.ID, file.Filename);

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
        public  IActionResult UploadArtifact([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!ArtifactExists(id))
                return NotFound();

            long size = 0;

            try
            {
                var artifact = _context.Artifacts.Single((art) => art.ID == id);
                string dirPath = Path.Combine(_storagePath, artifact.ID);

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                var files = Request.Form.Files;
                var sha2 = System.Security.Cryptography.SHA256.Create();

                foreach (var file in files)
                {
                    var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Value.Trim('"');

                    filename = Path.Combine(dirPath, filename);
                    size += file.Length;

                    var hashValue = "";

                    if (!IOF.Exists(filename))
                    {
                        using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }

                        hashValue = ComputeHash(sha2, filename);

                        _context.Storage.Add(new ArtifactStorageItem() { ArtifactRef = artifact, Filename = Path.GetFileName(filename), Hash = hashValue });
                        _context.SaveChanges();
                    }
                    else if (!artifact.StorageItems.Any(asi => asi.Filename == Path.GetFileName(filename)))
                    {
                        hashValue = ComputeHash(sha2, filename);

                        _context.Storage.Add(new ArtifactStorageItem() { ArtifactRef = artifact, Filename = Path.GetFileName(filename), Hash = hashValue });
                        _context.SaveChanges();
                    }
                }

                _context.Artifacts.Update(artifact);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

            return Ok(size);
        }

        private static string ComputeHash(System.Security.Cryptography.SHA256 sha2, string filename)
        {
            string hashValue;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                var byteHash = sha2.ComputeHash(fs);
                var stringHash = Convert.ToBase64String(byteHash);
                var stringHash2 = new String(System.Text.Encoding.UTF8.GetChars(byteHash));
                hashValue = stringHash;
            }

            return hashValue;
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
            else if (!ArtifactExists(id))
                return NotFound(id);

            var artifact = _context.Artifacts.Include(asi => asi.StorageItems).Single(ar => ar.ID == id);

            if (artifact.StorageItems?.Count == 0)
                return NotFound(filename);

            var file = artifact.StorageItems?.SingleOrDefault(asi => asi.Filename == filename);

            if (file == null)
                return NotFound(filename);
            else
            {
                var filePath = Path.Combine(_storagePath, artifact.ID, file.Filename);

                if (!IOF.Exists(filePath))
                    return NotFound("File does not exist on server!");
                else
                {
                    // Remove File
                    IOF.Delete(filePath);
                    // Remove Entries in StorageItems DB
                    artifact.StorageItems?.Remove(file);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
        }

        // DELETE: api/Artifacts/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtifact([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var artifact = await _context.Artifacts.SingleOrDefaultAsync(m => m.ID == id);

            if (artifact == null)
                return NotFound();

            _context.Artifacts.Remove(artifact);
            await _context.SaveChangesAsync();

            return Ok(artifact);
        }

        private bool ArtifactExists(string id)
        {
            return _context.Artifacts.Any(e => e.ID == id);
        }
    }
}