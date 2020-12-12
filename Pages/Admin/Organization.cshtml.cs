using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace budoco.Pages
{
    public class OrganizationModel : PageModel
    {

        // bindings start 

        [FromQuery]
        public int field { get; set; }

        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string name { get; set; }

        [BindProperty]
        public bool is_active { get; set; }

        [BindProperty]
        public bool is_default { get; set; }

        // bindings end        

        public void OnGet()
        {

            if (id == 0)
            {
                is_active = true;
            }
            else
            {
                string sql = @"select 
                og_id as ""id"",
                og_name as ""name"",
                og_is_active as ""is_active"",
                og_is_default as ""is_default""
                from organizations where og_id = " + id.ToString();

                DataRow dr = bd_db.get_datarow(sql);

                name = (string)dr["name"];
                is_active = (bool)dr["is_active"];
                is_default = (bool)dr["is_default"];
            }
        }

        public void OnPost()
        {
            if (!IsValid())
            {
                return;
            }

            if (id == 0)
            {
                string sql = @"insert into organizations
                (og_name, og_is_active, og_is_default)
                values (@name, @is_active, @is_default)
                returning og_id";

                var dict = new Dictionary<string, dynamic>();

                dict["@name"] = name;
                dict["@is_active"] = is_active;
                dict["@is_default"] = is_default;

                id = (int)bd_db.exec_scalar(sql, dict);
                bd_util.set_flash_msg(HttpContext, bd_util.CREATE_WAS_SUCCESSFUL);

            }
            else
            {

                string sql = @"update organizations set 
                og_name = @name,
                og_is_active = @is_active,
                og_is_default = @is_default
                where og_id = @id;";

                var dict = new Dictionary<string, dynamic>();

                dict["@id"] = id;
                dict["@name"] = name;
                dict["@is_active"] = is_active;
                dict["@is_default"] = is_default;

                bd_db.exec(sql, dict);
                bd_util.set_flash_msg(HttpContext, bd_util.UPDATE_WAS_SUCCESSFUL);

            }
        }

        bool IsValid()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errs.Add("Name is required.");
            }
            else
            {
                var sql = "select 1 from organizations where og_name = @name and og_id != @id";
                sql = sql.Replace("$", field.ToString());

                var dict = new Dictionary<string, dynamic>();
                dict["@name"] = name;
                dict["@id"] = id;

                if (bd_db.exists(sql, dict))
                {
                    errs.Add("Name is already in use.");
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
