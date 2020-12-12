/* 
    this deletes all reports in the db so be careful
*/ 

/* comment this out and then the rest of this sql is safe */
delete from reports;

/* Some examples to get you started */
insert into reports (rp_name, rp_sql, rp_chart_type)
    values('Issues by Status',
    'select c1_name "status", count(1) "count"
    from issues
    inner join custom_1 on i_custom_1 = c1_id
    group by c1_name
    order by c1_name',
    'pie');

insert into reports (rp_name, rp_sql, rp_chart_type)
    values('Issues by Priority',
    'select c2_name "priority", count(1) "count"
    from issues
    inner join custom_2 on i_custom_2 = c2_id
    group by c2_name
    order by c2_name',
    'pie');

insert into reports (rp_name, rp_sql, rp_chart_type)
    values('Issues by Category',
    'select c4_name "category", count(1) "count"
    from issues
    inner join custom_4 on i_custom_4 = c4_id
    group by c4_name
    order by c4_name',
    'pie');

insert into reports (rp_name, rp_sql, rp_chart_type)
    values('Issues by Month',
    'select DATE_PART(''month'', i_created_date) "month", count(1) "count"
    from issues
    group by DATE_PART(''year'', i_created_date), DATE_PART(''month'', i_created_date)
    order by DATE_PART(''year'', i_created_date), DATE_PART(''month'', i_created_date)',
    'bar');

insert into reports (rp_name, rp_sql, rp_chart_type)
    values('Issues by Day of Year',
    'select DATE_PART(''doy'', i_created_date) "day of year", count(1) "count"
    from issues
    group by DATE_PART(''doy'', i_created_date),
    DATE_PART(''doy'', i_created_date) order by 1',
    'line');

insert into reports (rp_name, rp_sql, rp_chart_type)
    values('Issues by User',
    'create temp table temp as
    select i_created_by_user, count(1) "r"
    from issues
    group by i_created_by_user;

    create temp table temp2 as
    select i_assigned_to_user, count(1) "a"
    from issues
    group by i_assigned_to_user;

    select us_username, r "reported", a "assigned"
    from users
    left outer join temp on i_created_by_user = us_id
    left outer join temp2 on i_assigned_to_user = us_id
    order by 1', 
    'table');

--insert into reports (rp_name, rp_sql, rp_chart_type)
--    values ('Hours by Org, Year, Month',
--    'select og_name "organization",
--    datepart(year,tsk_created_date) "year",
--    datepart(month,tsk_created_date) "month",
--    convert(decimal(8,1),sum(
--    case when tsk_duration_units = ''minutes'' then tsk_actual_duration / 60.0
--    when tsk_duration_units = ''days'' then tsk_actual_duration * 8.0
--    else tsk_actual_duration * 1.0 end)) "total hours"
--    from bug_tasks
--    inner join issues on tsk_bug = bg_id
--    inner join orgs on i_organization = og_id
--    where isnull(tsk_actual_duration,0) <> 0
--    group by og_name,datepart(year,tsk_created_date), datepart(month,tsk_created_date)',
--    'table');

--insert into reports (rp_name, rp_sql, rp_chart_type)
--    values ('Hours Remaining by Project',
--    'select pj_name "project",
--    convert(decimal(8,1),sum(
--    case
--    when tsk_duration_units = ''minutes'' then
--    tsk_planned_duration / 60.0 * .01 * (100 - isnull(tsk_percent_complete,0))
--    when tsk_duration_units = ''days'' then
--    tsk_planned_duration * 8.0  * .01 * (100 - isnull(tsk_percent_complete,0))
--    else tsk_planned_duration * .01 * (100 - isnull(tsk_percent_complete,0))
--    end)) "total hours"
--    from bug_tasks
--    inner join issues on tsk_bug = bg_id
--    inner join projects on bg_project = pj_id
--    where isnull(tsk_planned_duration,0) <> 0
--    group by pj_name',
--    'table');