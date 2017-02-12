using CAD_webapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CAD_webapp.Models;

namespace CAD_webapp.Controllers
{
    public class AllImagesController : Controller
    {
        // GET: AllImages
        public ActionResult ShowAllImages()
        {
            var temp = ImgToTheTable.RetrieveAllImages();
            return View(temp);
        }
    }
}