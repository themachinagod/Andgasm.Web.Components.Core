using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Andgasm.Web.Components.Core
{
    [Route("api/[controller]")]
    public class ResourceController : Controller
    {
        [ResponseCache(Duration = 86400)]
        [HttpGet("{resourcetype}/{resourcename}")]
        public Stream GetStreamContent(string resourcetype, string resourcename)
        {
            Type restype = FindTypeFromAssembly($"{resourcetype}ViewComponent");
            if (restype == null) throw new InvalidOperationException($"Specified resource type could not be resolved: {resourcetype}");

            var resname = $"{restype.Module.Name.Replace(".dll", "")}.Pages.Components.{restype.Name.Replace("ViewComponent", "")}.{resourcename}";
            if (resname == null) throw new InvalidOperationException($"Specified resource name could not be resolved: {resourcetype}");

            var stream = GetManifestResourceStream(restype.Assembly, resname);
            return stream;
        }

        private static Stream GetManifestResourceStream(Assembly assembly, string resource)
        {
            var correctname = assembly.GetManifestResourceNames().FirstOrDefault(x => x.ToLowerInvariant() == resource.ToLowerInvariant());
            return assembly.GetManifestResourceStream(correctname);
        }

        private static Type FindTypeFromAssembly(string classname)
        {
            var asmbs = AppDomain.CurrentDomain.GetAssemblies();
            var sourceassembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).
                                 FirstOrDefault(x => x.ExportedTypes.Any(y => y.Name.ToLowerInvariant().EndsWith(classname.ToLowerInvariant())));
            if (sourceassembly == null) return null;
            var asmblyname = sourceassembly.GetName().Name;
            return sourceassembly.GetType($"{asmblyname}.{classname}", false, true);
        }
    }
}