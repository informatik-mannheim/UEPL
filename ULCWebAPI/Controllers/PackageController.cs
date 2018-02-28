using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ULCWebAPI.Models;
using ULCWebAPI.Extensions;
using System;
using ULCWebAPI.Helper;
using ULCWebAPI.Attributes;

namespace ULCWebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [TokenPermissionRequired]
    [AllowWithoutToken(new string[] { "GET" })]
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

            foreach(var item in packageItem.Dependencies)
            {
                item.Dependencies.Clear();
            }

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

            var packages = _context.GetFullTable<Package>();

            var packageItem = await packages.SingleOrDefaultAsync(m => m.ID == id);
            var rPackageItem = await packages.SingleOrDefaultAsync(m => m.ID == rid);

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

            packageItem.Version++;

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

            if (!packageExists(id))
                return NotFound();

            var item = _context.Packages.First(pi => pi.ID == id);

            try
            {
                item.Name = packageItem.Name ?? item.Name;
                item.ArtifactRefID = packageItem.ArtifactRefID ?? item.ArtifactRefID;
                item.Version++;

                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        [HttpPost("{id}/dependencies")]
        public async Task<IActionResult> ChangeDependencies([FromRoute]int id, [FromBody] int[] dependencies)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var packages = _context.GetFullTable<Package>();
            var package = await packages.SingleOrDefaultAsync(p => p.ID == id);

            if (package == null)
                return NotFound();

            package.Dependencies.Clear();

            foreach(var dep in dependencies)
            {
                var depPack = await packages.SingleOrDefaultAsync(p => p.ID == dep);

                if (depPack == package) 
                    continue;
                else if (package.Dependencies.Contains(depPack))
                    continue;
                else
                    package.Dependencies.Add(depPack);
            }

            package.Version++;
            await _context.SaveChangesAsync();
            return AcceptedAtAction(nameof(GetPackageItem), new { id = package.ID }, package);
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

        private bool packageExists(int id) => _context.Packages.Any(e => e.ID == id);
    }
}