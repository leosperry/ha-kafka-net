using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaKafkaNet;

/// <summary>
/// wires up the physical files
/// </summary>
[Route("/hakafkanet/{*path}")]
public class HaKafkaNetController : Controller
{
    /// <summary>
    /// serves physical files
    /// </summary>
    /// <returns></returns>
    public ActionResult Index()
    {
        var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var path = Path.Combine(rootPath, "www/index.html");
        return PhysicalFile(path, "text/html");
    }

}
