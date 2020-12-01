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

            if (string.IsNullOrWhiteSpace(search_terms))
            {
                bd_util.set_flash_err(HttpContext, "Please enter word(s) to search");
                return;
            }

            string sql_template = @"
select i_id as ""ID"", i_description as ""Description"", context as ""Context"", max(rank) as score from (
select i_id, i_description, ts_headline('english', i_description, websearch_to_tsquery('english', '$')) as Context, rank from 
(
select i_id, i_description, 0 as p_id, '' as p_text, 
ts_rank(to_tsvector('english', i_description), websearch_to_tsquery('english', '$')) as rank
from issues 

union 

select i_id, i_description, p_id, p_text, 
ts_rank(to_tsvector('english', p_text), websearch_to_tsquery('english', '$')) 
from posts 
inner join issues on i_id = p_issue 
order by rank desc limit 20
) hits where rank > 0.01) best_hits
group by i_id, i_description, context
order by score, i_id desc";

            string escaped_single_quotes = search_terms.Replace("'", "''");
            string sql = sql_template.Replace("$", escaped_single_quotes);

            dt = bd_db.get_datatable(sql);

        }
    }
}
