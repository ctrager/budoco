using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace budoco.Pages
{
    public class SearchModel : PageModel
    {
        [FromQuery]
        public string search_terms { get; set; }

        public DataTable dt;

        public void OnGet()
        {

            if (string.IsNullOrWhiteSpace(search_terms))
            {
                //bd_util.set_flash_err(HttpContext, "Please enter word(s) to search.");
                return;
            }

            string sql_template = @"
                select
                    i_id as ""ID"",
                    i_description as ""Description"",
                    context as ""Context"", 
                    context_location,
                    p_id,
                    max(rank) as score
                from
                (
                    select
                        i_id,
                        i_description,
                        context_location,
                        p_id,
                        ts_headline('english', search_text, websearch_to_tsquery('english', '$'), 'StartSel=SSTART, StopSel=SSTOP') as Context,
                        rank 
                    from 
                    (
                        /* i_description */
                        select
                            i_id,
                            i_description,
                            'Description' as context_location,
                            0 as p_id,
                            i_description as search_text,
                            ts_rank_cd(to_tsvector('english', i_description), websearch_to_tsquery('english', '$')) as rank
                        from
                            issues
                        where
                            websearch_to_tsquery('english', '$') @@ to_tsvector('english', i_description)
                            /*AND_ORG*/
                       
                        union
                        /* i_details */
                        select
                            i_id,
                            i_description,
                            'Details' as context_location,
                            0 as p_id,
                            i_details as search_text,
                            ts_rank_cd(to_tsvector('english', i_details), websearch_to_tsquery('english', '$')) as rank
                        from
                            issues
                        where
                            websearch_to_tsquery('english', '$') @@ to_tsvector('english', i_details)
                            /*AND_ORG*/

                        union 
                        /* p_text */
                        select
                            i_id,
                            i_description,
                            'Post' as context_location,
                            p_id,
                            p_text as search_text,
                            ts_rank_cd(to_tsvector('english', p_text), websearch_to_tsquery('english', '$')) as rank
                        from
                            posts
                            
                            inner join issues
                            on i_id = p_issue
                        where
                            websearch_to_tsquery('english', '$') @@ to_tsvector('english', p_text)
                            /*AND_ORG*/
                        order by
                            rank desc limit 400
                    ) hits 
                    where
                        rank > 0.01
                ) best_hits
                group by
                    i_id,
                    i_description,
                    context_location,
                    p_id,
                    context
                order by
                    score desc,
                    p_id,
                    i_id desc";


            string escaped_single_quotes = search_terms.Replace("'", "''");
            string sql = sql_template.Replace("$", escaped_single_quotes);
            sql = bd_util.enhance_sql_per_user(HttpContext, sql);

            dt = bd_db.get_datatable(sql);

        }
    }
}
