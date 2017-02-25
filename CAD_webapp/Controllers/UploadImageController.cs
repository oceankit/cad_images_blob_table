using CAD_webapp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ImageResizer;
using System.Runtime.InteropServices.ComTypes;

namespace CAD_webapp.Controllers
{
    public class UploadImageController : Controller
    {
        // GET: UploadImage
        public ActionResult UploadImage()
        {
            UploadedImage defaultViewModel = new UploadedImage();
            return View(defaultViewModel);

        }

        private readonly IImageService _imageService = new ImageService();

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        [HttpPost]
        public async Task<ActionResult> Upload(FormCollection formCollection)
        {
            var model = new UploadedImage();

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["uploadedFile"];
                model = await _imageService.CreateUploadedImage(file);
                

                //var r = new ImageResizer.ResizeSettings() { MaxWidth = 220, MaxHeight = 160 };
                //file.InputStream.Seek(0, SeekOrigin.Begin);
                //string filePath = Server.MapPath(Url.Content("~/Content/Crop/" + "mini" + file.FileName));
                //ImageResizer.ImageBuilder.Current.Build(file, filePath, r);
                //Image b = Image.FromFile(filePath);
                //byte[] bytes = imageToByteArray(b);
                //mini_model.ContentType = file.ContentType;
                //mini_model.Name = "mini_" + file.FileName;
                //mini_model.Data = bytes;
                //mini_model.Url = string.Format("{0}/{1}", ConfigurationManager.AppSettings["ImageRootPath"], mini_model.Name);
                //await _imageService.AddImageToBlobStorageAsync(mini_model);

                ImageEntity ie = new ImageEntity();
                ie.Full_img = model.Url;
                //ie.Mini_img = mini_model.Url;
                ImgToTheTable.CreateIfNotExist(ie);
                await _imageService.AddImageToBlobStorageAsync(ie, model);
            }

            return View("UploadImage", model);
        }
    }
}