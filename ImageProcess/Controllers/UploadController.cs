using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace ImageProcess.Controllers
{
    public class UploadController : ApiController
    {

        
        //POST Method - Gets the Image from the Front End and Returns the Re-Sized Image(Half of the current Resolution)
        public Task<HttpResponseMessage> Post()
        {
            List<string> savedFilePath = new List<string>();
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            //Current uploaded Image and Re-Sized Image are stored in this location of server.
            string rootPath = HttpContext.Current.Server.MapPath("~/uploadFiles");
            var provider = new MultipartFileStreamProvider(rootPath);
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    foreach (MultipartFileData item in provider.FileData) //Reading the Image and storing it in the server path(Realtive path )
                    {
                        try
                        {
                            string name = item.Headers.ContentDisposition.FileName.Replace("\"", "");
                            //providing a new name for the Image - TO over-come the issue of Naming and Saving Complexity
                         string newFileName = Guid.NewGuid() + Path.GetExtension(name);
                            //Extracting the Extension from the ImageFile and saving -to provide same extension for the ReSized Image.
                            string extension = Path.GetExtension(name);
                            //Validating proper file(Images) or not 
                            if (extension.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".MemoryBmp", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".icon", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase))
                            {

                                File.Move(item.LocalFileName, Path.Combine(rootPath, newFileName));

                                Uri baseuri = new Uri(Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, string.Empty));
                                string fileRelativePath = "~/uploadFiles/" + newFileName;
                                Uri fileFullPath = new Uri(baseuri, VirtualPathUtility.ToAbsolute(fileRelativePath));

                                //path of the image
                                string imagepath = Path.Combine(rootPath, newFileName);
                                //Re-sizing Image begins Here
                                Image img = Image.FromFile(imagepath);

                                img = resizeimage(img, new Size(img.Width / 2, img.Height / 2));
                                string NewResolutionImage = Guid.NewGuid() + "_sairamrs" + Path.GetExtension(name);
                                img.Save(Path.Combine(rootPath, NewResolutionImage), ImageFormat.Png);

                                /// new file url creating
                                string newFileName_rs = "~/uploadFiles/" + NewResolutionImage;
                                Uri halfResolutionImagePath = new Uri(baseuri, VirtualPathUtility.ToAbsolute(newFileName_rs));
                                savedFilePath.Add(halfResolutionImagePath.ToString());
                            }
                            else
                            {
                                throw new Exception("Improper format");
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.Created, savedFilePath);
                });
            return task;
        }
        //TypeCasting image to BitMap(where reSizing is possible) and again form bitMap to Image
        private Image resizeimage(Image img, Size size)
        {
            return(Image)(new Bitmap(img, size));
        }
    }
}
