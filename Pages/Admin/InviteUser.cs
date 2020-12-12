using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace budoco.Pages
{
    public class InviteUserModel : PageModel
    {

        // bindings start 
        [BindProperty]
        public string email_address { get; set; }

        [BindProperty]
        public string username { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> organizations { get; set; }
        [BindProperty]
        public int organization_id { get; set; }

        // bindings end  
        List<string> errs = new List<string>();

        public void OnGet(string action)
        {
            organizations = bd_db.prepare_select_list("select og_id, og_name from organizations order by og_name");
        }

        public void OnPost()
        {

            if (!IsValid())
            {
                return;
            }

            // insert unguessable bytes into db for user to confirm registration
            string sql = @"insert into registration_requests 
            (rr_guid, rr_email_address, rr_username, rr_is_invitation, rr_organization)
            values(@rr_guid, @rr_email_address, @rr_username, @rr_is_invitation, @rr_organization)";

            var dict = new Dictionary<string, dynamic>();

            var guid = Guid.NewGuid();
            dict["@rr_guid"] = guid;
            dict["@rr_username"] = username; // on purpose, user can login typing either
            dict["@rr_email_address"] = email_address;
            dict["@rr_is_invitation"] = true;
            dict["@rr_organization"] = organization_id;

            bd_db.exec(sql, dict);

            // send an email
            // and tell user to check it

            string body = "You have been invited to use "
                + bd_config.get(bd_config.AppName)
                + " at "
                + bd_config.get(bd_config.WebsiteUrlRootWithoutSlash)
                + ". Follow or browse to this link to accept invite:\n"
                + bd_config.get(bd_config.WebsiteUrlRootWithoutSlash)
                + "/AcceptInvite?guid="
                + guid;

            bd_email.queue_email("invite",
                email_address, // to
                bd_config.get(bd_config.AppName) + ": Accept invitation", // subject
                body);

            bd_util.set_flash_msg(HttpContext, "Invitation has been placed in outgoing mail queue.");

            Response.Redirect("/Admin/InviteUser");

        }


        bool IsValid()
        {
            // DRY up this code 
            // Identica; in User and UserSettings
            // and similar in Register

            var dict = new Dictionary<string, dynamic>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("Username required.");
            }
            else
            {
                if (bd_util.is_username_already_taken(username))
                {
                    errs.Add("Username is used by somebody else.");
                }
            }

            if (string.IsNullOrWhiteSpace(email_address))
            {
                errs.Add("Email is required.");
            }
            else if (!bd_email.validate_email_address(email_address))
            {
                errs.Add("Email is invalid.");
            }
            else
            {
                if (bd_util.is_email_already_taken(email_address))
                {
                    errs.Add("Email is used by somebody else.");
                }
            }

            if (errs.Count == 0)
            {
                return true;
            }
            else
            {
                bd_util.set_flash_errs(HttpContext, errs);
                return false;
            }
        }
    }
}
