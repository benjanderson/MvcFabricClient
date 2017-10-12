using MvcFabricClient.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MvcFabricClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Secure()
        {
            var user = User as ClaimsPrincipal;
            var authClient = new AuthorizationClient(new Uri(ConfigurationManager.AppSettings["AuthorizationEndpoint"]), user);
            var securityService = new IdentitySecurityService(user, authClient);
            //ViewBag.IsAdmin = securityService.IsEdwAdmin;
            ViewBag.User = securityService.CurrentUserName;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}