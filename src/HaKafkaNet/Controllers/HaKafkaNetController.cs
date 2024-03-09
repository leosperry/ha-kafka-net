using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaKafkaNet;

[Route("/hakafkanet/{*path}")]
public class HaKafkaNetController : Controller
{
    public ActionResult Index()
    {
        var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var path = Path.Combine(rootPath, "www/index.html");
        return PhysicalFile(path, "text/html");
    }

}
