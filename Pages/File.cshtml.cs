using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using System;

namespace budoco.Pages
{
    public class FileModel : PageModel
    {
        [FromQuery]
        public int p_id { get; set; }

        public ActionResult OnGetFile()
        {

            if (!bd_util.check_user_permissions(HttpContext))
            {
                HttpContext.Response.StatusCode = 403;
                return null;
            }


            var sql = "select p_file_content_type from posts where p_id = " + p_id.ToString();

            DataRow dr = bd_db.get_datarow(sql);

            if (dr == null)
            {
                HttpContext.Response.StatusCode = 410; //"Gone"
                return null;
            }

            sql = "select pa_content from post_attachments where pa_post = " + p_id.ToString();
            byte[] bytea = bd_db.get_bytea(sql);

            //var cr = new ContentResult();

            //cr.ContentType = (string)dr["p_file_content_type"];
            //cr.Content = (bytea);
            return File(bytea, (string)dr["p_file_content_type"]);
        }
    }
}
