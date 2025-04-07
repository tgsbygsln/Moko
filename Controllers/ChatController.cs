using Microsoft.AspNetCore.Mvc;

public class ChatController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}