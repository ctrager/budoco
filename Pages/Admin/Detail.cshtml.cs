using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace budoco.Pages
{
    public class DetailModel : PageModel
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

        public string singular_label;
        public string plural_label;

        public void OnGet()
        {

            singular_label = bd_config.get("CustomFieldLabelSingular" + field.ToString());
            plural_label = bd_config.get("CustomFieldLabelPlural" + field.ToString());

            if (id == 0)
            {
                is_active = true;
            }
            else
            {
                string sql = @"select 
                c$_id as ""id"",
                c$_name as ""name"",
                c$_is_active as ""is_active"",
                c$_is_default as ""is_default""
                from custom_$ where c$_id = " + id.ToString();

                sql = sql.Replace("$", field.ToString());

                DataRow dr = bd_db.get_datarow(sql);

                name = (string)dr["name"];
                is_active = (bool)dr["is_active"];
                is_default = (bool)dr["is_default"];
            }
        }

        public void OnPost()
        {
            singular_label = bd_config.get("CustomFieldLabelSingular" + field.ToString());
            plural_label = bd_config.get("CustomFieldLabelPlural" + field.ToString());


            if (!IsValid())
            {
                return;
            }

            if (id == 0)
            {
                string sql = @"insert into custom_$ 
                (c$_name, c$_is_active, c$_is_default)
                values (@name, @is_active, @is_default)
                returning c$_id";

                sql = sql.Replace("$", field.ToString());

                var dict = new Dictionary<string, dynamic>();

                dict["@name"] = name;
                dict["@is_active"] = is_active;
                dict["@is_default"] = is_default;

                id = (int)bd_db.exec_scalar(sql, dict);
                bd_util.set_flash_msg(HttpContext, bd_util.CREATE_WAS_SUCCESSFUL);

            }
            else
            {

                string sql = @"update custom_$ set 
                c$_name = @name,
                c$_is_active = @is_active,
                c$_is_default = @is_default
                where c$_id = @id;";

                sql = sql.Replace("$", field.ToString());

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
                var sql = "select 1 from custom_$ where c$_name = @name and c$_id != @id";
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
