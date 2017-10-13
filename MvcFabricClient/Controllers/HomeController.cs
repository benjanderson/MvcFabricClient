using MvcFabricClient.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public async Task<ActionResult> Secure()
        {
            var user = User as ClaimsPrincipal;
            var authClient = new AuthorizationClient(ConfigurationManager.AppSettings["AuthorizationEndpoint"], user);
            var securityService = new IdentitySecurityService(authClient);
            ViewBag.IsAdmin = await securityService.IsEdwAdmin();
            ViewBag.User = IdentitySecurityService.CurrentUserName;
            return View();
        }

        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut("Cookies");
            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}