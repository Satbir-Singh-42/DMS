using System.Diagnostics;
using DMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class ErrorController : Controller
{
    [Route("Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        ViewBag.StatusCode = 500;
        ViewBag.StatusMessage = "An unexpected server error occurred while processing your request.";
        return View("Error", new ErrorViewModel { RequestId = requestId });
    }

    [Route("Error/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Status(int statusCode)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        ViewBag.StatusCode = statusCode;
        ViewBag.StatusMessage = statusCode switch
        {
            400 => "Bad Request: The server could not understand the request.",
            403 => "Access Denied: You do not have permission to view this resource.",
            404 => "Page Not Found: The requested resource or document does not exist.",
            500 => "Internal Server Error: A processing failure occurred.",
            _ => "An unexpected error occurred while processing your request."
        };
        return View("Error", new ErrorViewModel { RequestId = requestId });
    }
}

