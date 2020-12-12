using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace budoco.Pages
{
    public class LogsModel : PageModel
    {

        [FromQuery]
        public string filename { get; set; }

        [FromQuery]
        public int dl { get; set; }

        IWebHostEnvironment _env;
        public LogsModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string[] files;

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            string log_folder = GetFolderWithSlash();

            bd_util.log(log_folder);

            files = System.IO.Directory.GetFiles(log_folder);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = System.IO.Path.GetFileName(files[i]);
            }

            Array.Sort(files, StringComparer.InvariantCulture);


        } // end OnGet

        public ActionResult OnGetFile()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return null;

            string log_folder = GetFolderWithSlash();

            string full_path = log_folder + filename;

            byte[] bytea = System.IO.File.ReadAllBytes(full_path);

            if (dl == 1)
            {
                HttpContext.Response.Headers.Add("Content-Disposition", "download");
            }

            return File(bytea, "text/plain");
        }

        string GetFolderWithSlash()
        {
            return _env.ContentRootPath
                       + "/" + bd_config.get(bd_config.LogFileFolder) + "/";
        }
    }
}
