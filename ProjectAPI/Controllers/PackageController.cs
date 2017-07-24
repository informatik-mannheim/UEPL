using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Models;
using ProjectAPI.Extensions;
using System;
using ProjectAPI.Helper;

namespace ProjectAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PackageController : Controller
    {
        private readonly APIDatabaseContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public PackageController(APIDatabaseContext context) { _context = context; }

        // GET: api/Package
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Package> GetPackages() { return _context.Packages; }

        // GET: api/Package/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackageItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var packageItem = await _context.Packages.SingleOrDefaultAsync(m => m.ID == id);

            if (packageItem == null)
                return NotFound();

            return Ok(packageItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        [HttpGet("{id}/depend/{rid}")]
        public async Task<IActionResult> AddPackageDependency([FromRoute] int id, [FromRoute] int rid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var packageItem = await _context.Packages.Include(p => p.Dependencies).SingleOrDefaultAsync(m => m.ID == id);
            var rPackageItem = await _context.Packages.Include(p => p.Dependencies).SingleOrDefaultAsync(m => m.ID == rid);

            if (packageItem == null || rPackageItem == null)
                return NotFound();

            if (!packageItem.Dependencies.Contains(rPackageItem))
            {
                try
                {
                    packageItem.Dependencies.Add(rPackageItem);
                    _context.Packages.Update(packageItem);
                    await _context.SaveChangesAsync();
                }
                catch(Exception e)
                {
                    Tracer.TraceMessage(e.Message);
                    return StatusCode(500);
                }
            }

            return Ok(packageItem);
        }

        // PUT: api/Package/5
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="packageItem"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPackageItem([FromRoute] int id, [FromBody] Package packageItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!PackageItemExists(id))
                return NotFound();

            var item = _context.Packages.First(pi => pi.ID == id);

            try
            {
                item.Name = packageItem.Name ?? item.Name;
                item.ArtifactRefID = packageItem.ArtifactRefID ?? item.ArtifactRefID;

                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(item);
        }

        // POST: api/Package
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageItem"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostPackageItem([FromBody] Package packageItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!(await _context.Artifacts.AnyAsync((item) => item.ID == packageItem.ArtifactRefID)))
                await _context.Artifacts.AddAsync(new Artifact { ID = packageItem.ArtifactRefID, InstallAction = "", RemoveAction = "" });

            await _context.Packages.AddAsync(packageItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPackageItem), new { id = packageItem.ID }, packageItem);
        }

        // DELETE: api/Package/5
        /// <summary>
        /// deletes a package
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackageItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var packageItem = await _context.Packages.SingleOrDefaultAsync(m => m.ID == id);

            if (packageItem == null)
                return NotFound();

            _context.Packages.Remove(packageItem);
            await _context.SaveChangesAsync();

            return Ok(packageItem);
        }

        private bool PackageItemExists(int id) { return _context.Packages.Any(e => e.ID == id); }
    }
}