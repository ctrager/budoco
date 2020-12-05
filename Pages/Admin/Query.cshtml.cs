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
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Linq;

namespace budoco.Pages
{
    public class QueryModel : PageModel
    {

        private readonly IRazorPartialToStringRenderer _renderer;

        public QueryModel(IRazorPartialToStringRenderer renderer)
        {
            _renderer = renderer;
        }


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
        public string success;

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
                errs.Add(bd_util.NAME_IS_REQUIRED);
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                errs.Add("Sql is required.");
            }
            else
            {
                if (!sql.ToLower().Contains(" issues"))
                {
                    errs.Add(@"Sql must reference the table ""issues"".");
                }

                string dangerous_word = FindDangerousWordInSql(sql);

                if (dangerous_word is not null)
                {
                    errs.Add("Dangerous keyword not allowed (see Budoco DangerousSqlKeywords setting): " + dangerous_word);
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

        // Same as RunSql.cshmtl.cs but tries to detect sql that does an update
        // No promises though. You need to trust your admins.
        public async Task<ContentResult> OnPostRunAsync()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return Content("<div>Must be admin</div>");

            dt = null;
            error = null;

            if (string.IsNullOrWhiteSpace(sql))
            {
                error = "Sql is required.";
            }
            else
            {
                // sql is not blank
                string dangerous_word = FindDangerousWordInSql(sql);

                if (dangerous_word is not null)
                {
                    error = "Dangerous keyword not allowed (see Budoco DangerousSqlKeywords setting): " + dangerous_word;
                }
                else
                {
                    // sql has no dangerous words, so try to run
                    try
                    {
                        dt = bd_db.get_datatable(sql);
                    }
                    catch (Exception e)
                    {

                        if (e.Message == "Cannot find table 0.")
                        {
                            // suppress this - this is just what happens when we run a query that does't SELECT 
                            // like an update   
                            success = "Success";
                        }
                        else
                        {
                            error = e.Message;
                        }
                    }
                }
            }
            String html = await _renderer.RenderPartialToStringAsync("_PlainDataTablePartial", this);
            return Content(html);
        }

        string FindDangerousWordInSql(string sql)
        {

            if (bd_config.get(bd_config.CheckForDangerousSqlKeywords) == 0)
                return null;

            // build array of keywords
            string[] dangerous_keywords = bd_config.get(bd_config.DangerousSqlKeywords).ToLower().Split(",");
            string s = sql.Trim().ToLower();

            // convert some chars to whitespace, like ,;=:

            // [^ means not, so if th
            Regex rgx = new Regex("[^a-z0-9\"\' _]");
            s = rgx.Replace(s, " ");
            string[] sql_words = s.Split(" ");

            // find the first dangerous word
            foreach (string word_in_query in sql_words)
            {
                if (dangerous_keywords.Contains(word_in_query))
                {
                    return word_in_query;
                }
            }

            return null;
        }

    }
}
