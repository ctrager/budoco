using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace budoco.Pages
{
    public class SearchModel : PageModel
    {
        [BindProperty]
        public string search_terms { get; set; }

        public DataTable dt;

        public void OnGet()
        {

            // if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
            //     return;
        }

        public void OnPost()
        {
            string sql_template = @"
select i_id as ""ID"", i_description as ""Description"", context as ""Context"", max(rank) as score from (
select i_id, i_description, ts_headline('english', i_description, to_tsquery('$')) as Context, rank from 
(
select i_id, i_description, 0 as p_id, '' as p_text, 
ts_rank(to_tsvector(i_description), to_tsquery('$')) as rank
from issues 

union 

select i_id, i_description, p_id, p_text, 
ts_rank(to_tsvector(p_text), to_tsquery('$')) 
from posts 
inner join issues on i_id = p_issue 
order by rank desc limit 20
) hits where rank > 0.01) best_hits
group by i_id, i_description, context
order by score, i_id desc";

            string sql = sql_template.Replace("$", search_terms);

            dt = bd_db.get_datatable(sql);

        }
    }
}
