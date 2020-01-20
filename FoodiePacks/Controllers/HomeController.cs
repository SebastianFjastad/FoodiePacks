using System.Collections.Generic;
using System.Web.Mvc;
using FoodiePacks.Models;

namespace FoodiePacks.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var manager = new OrderManager();
            ViewBag.ProductData = manager.Download();
            return View();
        }

        public ActionResult Email()
        {
            var manager = new OrderManager();
            manager.SendWeeklyOrderEmail();
            return View();
        }


        public ActionResult Test()
        {
            return View();
        }
    }
}