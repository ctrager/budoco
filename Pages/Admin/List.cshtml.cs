using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data;




namespace budoco.Pages
{
    public class ListModel : PageModel
    {

        [FromQuery]
        public int field { get; set; }

        public DataTable dt;
        public string plural_label;
        public string singular_label;
        public string singular_table_name = "Detail";

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            plural_label = bd_config.get("CustomFieldLabelPlural" + field.ToString());
            singular_label = bd_config.get("CustomFieldLabelSingular" + field.ToString());

            string sql = @"select 
            c$_id as ""ID"",
            c$_name as ""Name"",
            c$_is_active as ""Active"",
            c$_is_default as ""Default""
            from custom_$
            order by c$_name";

            sql = sql.Replace("$", field.ToString());

            dt = bd_db.get_datatable(sql);
        }
    }
}
