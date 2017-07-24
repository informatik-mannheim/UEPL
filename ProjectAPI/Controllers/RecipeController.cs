using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Models;
using System;

namespace ProjectAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class RecipeController : Controller
    {
        private APIDatabaseContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public RecipeController(APIDatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Lecture Short (ID)</param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/install")]
        public async Task<IActionResult> GetInstallScript([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();
            else
                return Ok(await GenerateResponse(id, ResolveType.Install));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/download")]
        public async Task<IActionResult> GetDownloadScript([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();
            else
                return Ok(await GenerateResponse(id, ResolveType.Download));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/remove")]
        public async Task<IActionResult> GetRemoveScript([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();
            else
                return Ok(await GenerateResponse(id, ResolveType.Remove));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/switch")]
        public async Task<IActionResult> GetSwitchScript([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();
            else
                return Ok(await GenerateResponse(id, ResolveType.Switch));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/unswitch")]
        public async Task<IActionResult> GetUnswitchScript([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();
            else
                return Ok(await GenerateResponse(id, ResolveType.Unswitch));
        }

        private async Task<string> GenerateResponse(string id, ResolveType resolveType)
        {
            var lectureItem = await _context.Lectures.Include(l => l.Contents).ThenInclude(p => p.Dependencies).SingleOrDefaultAsync((item) => item.ID == id);
            var packageListResolved = new List<int>();
            var packageInstallCommands = new List<string>();

            if (lectureItem == null)
                return String.Empty;

            foreach (var package in lectureItem.Contents)
            {
                if (packageListResolved.Contains(package.ID))
                    continue;

                packageListResolved.Add(package.ID);
                packageInstallCommands.AddRange(resolve(package, resolveType));
            }

            return string.Join(" && ", packageInstallCommands);
        }

        private List<string> resolve(Package p, ResolveType resolveType)
        {
            List<string> sub = new List<string>();

            if (p.Dependencies == null || p.Dependencies.Count == 0)
            {
                foreach (var artifact in _context.Artifacts.Include(asi => asi.StorageItems).Where((art) => art.ID == p.ArtifactRefID))
                {
                    if(resolveType.HasFlag(ResolveType.Download))
                    {
                        artifact.StorageItems?.ForEach(asi =>
                        {
                            var url = $"{Request.Scheme}://{Request.Host}/api/artifact/{artifact.ID}/file/{asi.Filename}";
                            sub.Add($"URL={url}{Environment.NewLine}[ ! -e \"$(basename $URL)\" ] && wget -q -N $URL");
                        });
                    }

                    if(resolveType.HasFlag(ResolveType.Install))
                        sub.Add(artifact.InstallAction);

                    if (resolveType.HasFlag(ResolveType.Remove))
                        sub.Add(artifact.RemoveAction);

                    if (resolveType.HasFlag(ResolveType.Switch))
                        sub.Add(artifact.SwitchAction);

                    if (resolveType.HasFlag(ResolveType.Unswitch))
                        sub.Add(artifact.UnswitchAction);
                }
            }
            else
            {
                foreach (var subpackage in p.Dependencies)
                {
                    if (subpackage == null)
                        continue;

                    sub.AddRange(resolve(subpackage, resolveType));
                }
            }

            return sub;
        }

        [Flags]
        private enum ResolveType { Install = 1, Download = 2, Remove = 4, Switch = 8, Unswitch = 16 };
    }
}