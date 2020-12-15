using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Data;
using System.Collections.Generic;


// This file is not in the "App" folder because we don't
// want the bd_util.check_user_permissions logic to require
// a logged in user
namespace budoco.Pages
{

    /*
    This "IgnoreAntiforgeryToken" allows the screenshot app to post
    */
    [IgnoreAntiforgeryToken]
    public class CreateIssueModel : PageModel
    {

        class MyResult
        {
            public bool is_success { get; set; } = false;
            public string error { get; set; }
            public int issue_id { get; set; }
            public int post_id { get; set; }
        };

        public JsonResult OnPost()
        {
            var result = new MyResult();

            string username = HttpContext.Request.Form["username"];
            string password = HttpContext.Request.Form["password"];
            string description = HttpContext.Request.Form["description"];
            string image_data = HttpContext.Request.Form["image_data"];

            if (string.IsNullOrEmpty(username))
            {
                result.error = "username missing";
                return new JsonResult(result);
            }

            if (string.IsNullOrEmpty(username))
            {
                result.error = "password missing";
                return new JsonResult(result);
            }

            if (string.IsNullOrEmpty(username))
            {
                result.error = "description missing";
                return new JsonResult(result);
            }

            string sql = "select us_id, us_is_active, us_password from users where us_username = @username";
            var dict = new Dictionary<string, dynamic>();
            dict["@username"] = username;
            DataRow dr = bd_db.get_datarow(sql, dict);

            if (dr is null)
            {
                result.error = "invalid username or password";
                return new JsonResult(result);
            }

            string password_in_db = (string)dr["us_password"];

            if (!bd_util.check_password_against_hash(password, password_in_db))
            {
                result.error = "invalid username or password";
                return new JsonResult(result);
            }

            if (!(bool)dr["us_is_active"])
            {
                result.error = "username is inactive";
                return new JsonResult(result);
            }

            int issue_id = bd_issue.create_issue(description, "posted via API", (int)dr["us_id"]);

            // user user_id 1 for now
            int post_id = bd_issue.create_comment_post_with_image(issue_id, 1, image_data);

            result.is_success = true;
            result.issue_id = issue_id;
            result.post_id = post_id;

            return new JsonResult(result);
        }


    }
}
