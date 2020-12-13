using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace budoco.Pages
{

    /*

    This allows the screenshot app to post

    */
    [IgnoreAntiforgeryToken]
    public class CreateIssueModel : PageModel
    {

        class MyResult
        {
            public string result { get; set; }
            public int issue_id { get; set; }
            public int post_id { get; set; }
        };

        public JsonResult OnPost()
        {

            string username = HttpContext.Request.Form["username"];
            string password = HttpContext.Request.Form["password"];
            string description = HttpContext.Request.Form["description"];
            string image_data = HttpContext.Request.Form["image_data"];


            int issue_id = bd_issue.create_issue(description, "posted by Budoco screenshot", 1);

            // user user_id 1 for now
            int post_id = bd_issue.create_comment_post_with_image(issue_id, 1, image_data);


            // This works
            //System.IO.File.WriteAllBytes("MY_IMAGE.PNG", binData);




            var result = new MyResult();
            result.result = "success";
            result.issue_id = issue_id;
            result.post_id = post_id;

            var json = JsonSerializer.Serialize<MyResult>(result);
            return new JsonResult(json);
        }
    }
}
