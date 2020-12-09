using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace budoco.Pages
{
    public class FileUploadModel : PageModel
    {
        [BindProperty]
        public IFormFile UploadedFile { get; set; }


        [BindProperty]
        public string my_string { get; set; }

        public void OnPost()
        {
            Console.WriteLine(my_string);

            foreach (var k in HttpContext.Request.Form.Keys)
            {
                Console.WriteLine(k);
                Console.WriteLine(HttpContext.Request.Form[k]);
            }

            if (UploadedFile is not null)
            {
                Console.WriteLine(UploadedFile.Length);
                Console.WriteLine(UploadedFile.ContentType);
                Console.WriteLine(UploadedFile.FileName);

                Console.WriteLine(UploadedFile.Name);

                MemoryStream s = new MemoryStream();
                UploadedFile.CopyTo(s);
                Console.WriteLine(s.Length);
                s.Close();
                Console.WriteLine(s.ToArray().Length);
            }
        }
    }
}

