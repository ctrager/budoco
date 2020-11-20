truncate table queries;

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'All issues',
'select i_id, i_desc, i_project, i_category, i_status, i_priority, i_created_by_user, i_assigned_to_user'
|| chr(10) || ' from issues '
|| chr(10) || ' order by i_id desc',
1);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'All open issues',
'select i_id, i_desc, i_status '
|| chr(10) || ' from issues '
|| chr(10) || ' where (i_status != 3) '
|| chr(10) || ' order by i_id desc',
2);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'Issues assigned to me',
'select i_id, i_desc '
|| chr(10) || ' from issues '
|| chr(10) || ' where (i_assigned_to_user = $ME) '
|| chr(10) || ' order by i_id desc',
2);

