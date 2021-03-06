using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ULCWebAPI.Models;
using System;
using System.Security.Cryptography;

using ENV = System.Environment;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using ULCWebAPI.Attributes;
using ULCWebAPI.Helper;

namespace ULCWebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [SignData]
    public class RecipeController : Controller
    {
        private const string NL = "\n";

        // TODO: Refactor this. The logic should be used by the service after determining which os it runs...
        private static string DownloadScript = $"URL=$!URL!${NL}FILENAME=$(basename $URL){NL}if [ -e $FILENAME ]; then {NL}echo 'File ' + $FILENAME + ' exists... skipping download';{NL}else{NL}wget -q $URL{NL}fi{NL}";

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
        /// <param name="id"></param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/meta")]
        public async Task<IActionResult> GetMeta([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();

            try
            {
                var downloadAction = await generateResponse(id, ResolveType.Download);
                var installAction = await generateResponse(id, ResolveType.Install);
                var removeAction = await generateResponse(id, ResolveType.Remove);
                var switchAction = await generateResponse(id, ResolveType.Switch);
                var unswitchAction = await generateResponse(id, ResolveType.Unswitch);
                var upgradeAction = await generateResponse(id, ResolveType.Upgrade);

                StringBuilder builder = new StringBuilder();

                SHA256 sha2 = SHA256.Create();
                builder.AppendLine($"Download = {getHash(downloadAction, sha2)}");
                builder.AppendLine($"Install = {getHash(installAction, sha2)}");
                builder.AppendLine($"Remove = {getHash(removeAction, sha2)}");
                builder.AppendLine($"Switch = {getHash(switchAction, sha2)}");
                builder.AppendLine($"Unswitch = {getHash(unswitchAction, sha2)}");
                builder.AppendLine($"Upgrade = {getHash(upgradeAction, sha2)}");

                var lectureItem = await _context.Lectures.Where(l => l.ID == id).Include(l => l.StorageItems).Include(l => l.Contents).ThenInclude(p => p.Dependencies).SingleOrDefaultAsync((item) => item.ID == id);

                foreach (var package in lectureItem.Contents)
                {
                    var artifact = await _context.Artifacts.Include(a => a.StorageItems).SingleOrDefaultAsync(item => item.ID == package.ArtifactRefID);

                    if (artifact == null)
                        continue;
                    else
                    {
                        foreach (var file in artifact.StorageItems)
                        {
                            builder.AppendLine($"{file.Filename} = {file.Hash}");
                        }
                    }
                }

                var data = builder.ToString();
                return Ok(data);

            }
            catch(Exception e)
            {
                Tracer.TraceMessage(e);
                throw e;
            }
        }

        private static string getHash(string action, SHA256 sha2)
        {
            byte[] hashBytes = sha2.ComputeHash(Encoding.UTF8.GetBytes(action));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Lecture Short (ID)</param>
        /// <returns></returns>
        [Produces("text/plain")]
        [HttpGet("{id}/install")]
        // TODO: Check installed contexts to identify missing packages and just send those install scripts and files
        public async Task<IActionResult> GetInstallScript([FromRoute] string id)
        {
            if (!_context.Lectures.Any(l => l.ID == id))
                return NotFound();
            else
                return Ok(await generateResponse(id, ResolveType.Install));
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
            {
                var response = await generateResponse(id, ResolveType.Download);
                return Ok(response);
            }
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
                return Ok(await generateResponse(id, ResolveType.Remove));
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
                return Ok(await generateResponse(id, ResolveType.Switch));
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
                return Ok(await generateResponse(id, ResolveType.Unswitch));
        }

        private async Task<string> generateResponse(string id, ResolveType resolveType)
        {
            var lectureItem = await _context.Lectures.Include(l => l.StorageItems).Include(l => l.Contents).ThenInclude(p => p.Dependencies).SingleOrDefaultAsync((item) => item.ID == id);
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

            if(resolveType == ResolveType.Download)
            {
                lectureItem.StorageItems?.ForEach(lsi =>
                {
                    var url = $"{Request.Scheme}://{Request.Host}/api/lecture/{lectureItem.ID}/file/{lsi.Filename}";
                    packageInstallCommands.Add(DownloadScript.Replace("$!URL!$", url));
                });
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
                            sub.Add(DownloadScript.Replace("$!URL!$", url));
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
        private enum ResolveType { Install = 1, Download = 2, Remove = 4, Switch = 8, Unswitch = 16, Upgrade = 32 };
    }
}