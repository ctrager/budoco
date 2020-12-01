using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RazorPartialToString.Services;
using System.Data.Common;

namespace budoco.Pages
{
    public class QueryModel : PageModel
    {

        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string name { get; set; }
        [BindProperty]
        public string description { get; set; }
        [BindProperty]
        public string sql { get; set; }

        [BindProperty]
        public bool is_active { get; set; }

        [BindProperty]
        public bool is_default { get; set; }

        public DataTable dt;

        public string error;

        public void OnGet()
        {

            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            if (id == 0)
            {
                is_active = true;

            }
            else
            {
                DataRow dr = bd_db.get_datarow("select * from queries where qu_id = " + id.ToString());

                sql = (string)dr["qu_sql"];
                name = (string)dr["qu_name"];
                description = (string)dr["qu_description"];
                is_active = (bool)dr["qu_is_active"];
                is_default = (bool)dr["qu_is_default"];
            }
        }

        public void OnPost()
        {

            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            if (!IsValid())
            {
                return;
            }

            if (description is null)
            {
                description = "";
            }

            if (id == 0)
            {
                var insert_sql = @"insert into queries 
                (qu_name, qu_description, qu_sql, qu_is_default, qu_is_active)
                values(@qu_name, @qu_description, @qu_sql, @qu_is_default, @qu_is_active)
                returning qu_id";

                id = (int)bd_db.exec_scalar(insert_sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, bd_util.CREATE_WAS_SUCCESSFUL);

            }
            else
            {
                var update_sql = @"update queries set 
                qu_name = @qu_name,
                qu_description = @qu_description,
                qu_sql = @qu_sql,
                qu_is_default = @qu_is_default,
                qu_is_active = @qu_is_active
                where qu_id = @qu_id;";

                bd_db.exec(update_sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, bd_util.UPDATE_WAS_SUCCESSFUL);

            }

            Response.Redirect("Query?id=" + id.ToString());
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@qu_id"] = id;
            dict["@qu_name"] = name;
            dict["@qu_description"] = description;
            dict["@qu_sql"] = sql;
            dict["@qu_is_default"] = is_default;
            dict["@qu_is_active"] = is_active;

            return dict;
        }

        bool IsValid()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errs.Add("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                errs.Add("Sql is required");
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
