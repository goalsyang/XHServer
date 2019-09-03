using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManageServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [Consumes("application/json", "multipart/form-data")]//此处为新增
    public class FileController: Controller
    {


        private readonly IHostingEnvironment _hostingEnvironment;

        public FileController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("iform")]
        public ResultObject UploadIForm(List<IFormFile> files)
        {
            List<String> filenames = new List<string>(); foreach (var file in files)
            {
                var fileName = file.FileName;
                Console.WriteLine(fileName);

                fileName = $"/UploadFile/{fileName}";
                filenames.Add(fileName);

                fileName = _hostingEnvironment.WebRootPath + fileName;

                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }

            return new ResultObject
            {
                state = "Success",
                resultObject = filenames
            };

        }

        public class ResultObject
        {
            public String state { get; set; }
            public Object resultObject { get; set; }
        }






        [HttpGet]
        public string APosts()
        {
            return "ok";
        }



        [HttpPost]
        public ActionResult UploadFile()
        {
            try
            {
                string Id = "";//记录返回的附件Id
               // string filePath = "";//记录文件路径

                byte[] a = new byte[(int)Request.ContentLength];
                Request.Body.Read(a, 0, a.Length);
                string res = Encoding.Default.GetString(a);


                Stream stream = HttpContext.Request.Body;
                byte[] buffer = new byte[HttpContext.Request.ContentLength.Value];
                stream.Read(buffer, 0, buffer.Length);
                /*string content = Encoding.UTF8.GetString(buffer);

                IFormFileCollection formFiles = Request.Form.Files;//获取上传的文件
                if (formFiles == null || formFiles.Count == 0)
                {
                    return Json(new { status = -1, message = "没有上传文件", filepath = "" });
                }
                IFormFile file = formFiles[0];
                string fileExtension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);//获取文件名称后缀 
                //保存文件
                 stream = file.OpenReadStream();
                // 把 Stream 转换成 byte[] 
                byte[] bytes = new byte[stream.Length];*/
                //stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始 
                //stream.Seek(0, SeekOrigin.Begin);
                // 把 byte[] 写入文件 
                FileStream fs = new FileStream("D:\\" +"aax.doc", FileMode.CreateNew);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(buffer);
                bw.Close();
                fs.Close();
                return Json(new { success = true, status = 0, message = "上传成功", filepath = "D:\\" + "aa", code = Id });
            }

            catch (Exception ex)
            {
                return Json(new { success = false, status = -3, message = "上传失败", data = ex.Message, code = "" });
            }
        }






        [HttpPost("iform")]
        public String PostFile([FromForm] IFormCollection formCollection)
        {
            String result = "Fail";
            if (formCollection.ContainsKey("user"))
            {
                var user = formCollection["user"];
            }
            FormFileCollection fileCollection = (FormFileCollection)formCollection.Files;
            foreach (IFormFile file in fileCollection)
            {
                StreamReader reader = new StreamReader(file.OpenReadStream());
                String content = reader.ReadToEnd();
                String name = file.FileName;
                String filename = @"D:/Test/" + name;
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    // 复制文件
                    file.CopyTo(fs);
                    // 清空缓冲区数据
                    fs.Flush();
                }
                result = "Success";
            }
            return result;
        }
    }
}
