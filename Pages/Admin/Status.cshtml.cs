using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Razor.Language;

namespace budoco.Pages
{
    public class StatusModel : PageModel
    {

        // bindings start 
        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string name { get; set; }

        [BindProperty]
        public bool is_active { get; set; }

        [BindProperty]
        public bool is_default { get; set; }

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            if (id != 0)
            {
                string sql = @"select * from statuses where st_id = " + id.ToString();
                DataRow dr = bd_db.get_datarow(sql);

                if (dr != null)
                {
                    name = (string)dr[1];
                    is_active = (bool)dr[2];
                    is_default = (bool)dr[3];
                }
                else
                {
                    bd_util.set_flash_err(HttpContext, "Status not found:" + id.ToString());
                    Response.Redirect("Statuses");
                }
            }
        }

        public void OnPost()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                bd_util.set_flash_err(HttpContext, "Name is required");
                return;
            }

            var dict = GetValuesDict();

            bool name_already_used = bd_db.exists("select 1 from statuses where st_name = @name", dict);
            if (name_already_used)
            {
                bd_util.set_flash_err(HttpContext, "Name is already being used");
                return;
            }

            string sql;
            if (id == 0)
            {
                sql = @"insert into statuses
                (st_name, st_is_active, st_is_default)
                values (@name, @is_active, @is_default)
                returning st_id";
                id = (int)bd_db.exec_scalar(sql, dict);
                bd_util.set_flash_err(HttpContext, "Create was successful");

            }
            else
            {
                sql = @"update statuses set
                st_name = @name,
                st_is_active = @is_active,
                st_is_default = @is_default
                where st_id = @id";
                bd_db.exec(sql, dict);
                bd_util.set_flash_err(HttpContext, "Update was successful");

            }
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@id"] = id;
            dict["@name"] = name;
            dict["@is_active"] = is_active;
            dict["@is_default"] = is_default;

            return dict;
        }
    }
}
