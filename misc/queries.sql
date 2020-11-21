truncate table queries;

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'All issues',
'select i_id, i_desc, i_project, i_category, i_status, i_priority, i_created_by_user, i_assigned_to_user'
|| chr(10) || ' from issues '
|| chr(10) || ' order by i_id desc',
1);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'All open issues',
'select i_id as "ID", i_desc as "Descripton", i_status as "Status", us_username as "Assigned to" '
|| chr(10) || ' from issues '
|| chr(10) || ' left outer join statuses on st_id = i_status'
|| chr(10) || ' left outer join users on us_id = i_assigned_to_user'
|| chr(10) || ' where (i_status != 3) '
|| chr(10) || ' order by i_id desc',
2);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'Issues assigned to me',
'select i_id as "ID", i_desc as "Description", st_name as "Status"'
|| chr(10) || ' from issues '
|| chr(10) || ' left outer join statuses on st_id = i_status'
|| chr(10) || ' where (i_assigned_to_user = $ME) '
|| chr(10) || ' order by i_id desc',
2);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'Many joins',
'select i_id as "ID", ' 
|| chr(10) || ' i_desc as "DESC", pj_name as "Project", ca_name as "Category", cr8.us_username as "Created by",'
|| chr(10) || ' i_created_date "Created", pr_name as "Priority", asg.us_username as "Assigned To",'
|| chr(10) || ' st_name as "Status", lu.us_username as "Last Changed By", i_last_updated_date as "Last Update"'
|| chr(10) || ' from issues '
|| chr(10) || ' left outer join users cr8 on cr8.us_id = i_created_by_user'
|| chr(10) || ' left outer join users asg on asg.us_id = i_assigned_to_user'
|| chr(10) || ' left outer join users lu on lu.us_id = i_last_updated_user'
|| chr(10) || ' left outer join projects on pj_id = i_project'
|| chr(10) || ' left outer join categories on ca_id = i_category'
|| chr(10) || ' left outer join priorities on pr_id = i_priority'
|| chr(10) || ' left outer join statuses on st_id = i_status'
|| chr(10) || ' order by i_id desc',
99);

