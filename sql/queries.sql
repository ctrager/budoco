
/* 
this deletes all queries in the db so be careful
*/ 

/* comment this out and then the rest of this sql is safe */
delete from queries;

/* all */
insert into queries (qu_name, qu_is_default, qu_description, qu_sql) values (
'All issues', 
true, 

'All the issue table columns, all the joins. '
|| chr(10) || ' If this causes the Issue page to be too crowded, edit the SQL to delete to delete the columns '
|| chr(10) || ' (and maybe the joins) you don''t want. Notice that this is the default query. You control that. ',

'select i_id as "ID", i_description as "DESC", asg.us_username as "Assigned To",' 
|| chr(10) || ' cr8.us_username as "Created by",'
|| chr(10) || ' og_name as "Organization ",'
|| chr(10) || ' c1_name as "Status ",'
|| chr(10) || ' c2_name as "Priority ",'
|| chr(10) || ' c4_name as "Category ",'
|| chr(10) || ' lu.us_username as "Last Updated By",'
|| chr(10) || ' i_last_updated_date as "Last Update",'
|| chr(10) || ' lp.us_username as "Last Post By",'
|| chr(10) || ' i_last_post_date as "Last Post" '
|| chr(10) || ' from issues'
|| chr(10) || ' left outer join users cr8 on cr8.us_id = i_created_by_user'
|| chr(10) || ' left outer join users asg on asg.us_id = i_assigned_to_user'
|| chr(10) || ' left outer join users lu on lu.us_id = i_last_updated_user'
|| chr(10) || ' left outer join users lp on lp.us_id = i_last_post_user'
|| chr(10) || ' left outer join organizations on og_id = i_organization'
|| chr(10) || ' left outer join custom_1 on c1_id = i_custom_1'
|| chr(10) || ' left outer join custom_2 on c2_id = i_custom_2'
|| chr(10) || ' left outer join custom_3 on c3_id = i_custom_3'
|| chr(10) || ' left outer join custom_4 on c4_id = i_custom_4'
|| chr(10) || ' left outer join custom_5 on c5_id = i_custom_5'
|| chr(10) || ' left outer join custom_6 on c6_id = i_custom_6'
|| chr(10) || ' order by i_id desc');


/* open */
insert into queries (qu_name, qu_description, qu_sql) values (
'Open issues',

'Only open issues. Notice the "where st_status != 3" clause. '
|| chr(10) || ' The app doesn''t actually "know" what statuses are or anything about workflow. '
|| chr(10) || ' You control the initial status by making one the default. In the initial setup the default is '
|| chr(10) || ' the status "new" with an id of 1.  There is also a status "done" with an id of 3. This query '
|| chr(10) || ' omits the 3''s. Make up whatever statuses you want and name them whatever you want.',

'select i_id as "ID", i_description as "DESC", c1_name as "Status", asg.us_username as "Assigned To",' 
|| chr(10) || ' cr8.us_username as "Created by",'
|| chr(10) || ' og_name as "Organization ",'
|| chr(10) || ' lu.us_username as "Last Updated By",'
|| chr(10) || ' i_last_updated_date as "Last Update",'
|| chr(10) || ' lp.us_username as "Last Post By",'
|| chr(10) || ' i_last_post_date as "Last Post"'
|| chr(10) || ' from issues'
|| chr(10) || ' left outer join users cr8 on cr8.us_id = i_created_by_user'
|| chr(10) || ' left outer join users asg on asg.us_id = i_assigned_to_user'
|| chr(10) || ' left outer join users lu on lu.us_id = i_last_updated_user'
|| chr(10) || ' left outer join users lp on lp.us_id = i_last_post_user'
|| chr(10) || ' left outer join organizations on og_id = i_organization'
|| chr(10) || ' left outer join custom_1 on c1_id = i_custom_1'
|| chr(10) || ' where i_custom_1 != 3'
|| chr(10) || ' order by i_id desc');


/* assigned to me */
insert into queries (qu_name, qu_description, qu_sql) values (
'Open issues assigned to me',

'Notice the "$ME" in the query. The app replaces that with the id of the logged in user.'
|| chr(10) || ' Here we are using it to show only the issues assigned to the logged in user.',

'select i_id as "ID", i_description as "DESC", c1_name as "Status", ' 
|| chr(10) || ' cr8.us_username as "Created by",'
|| chr(10) || ' c2_name as "Priority",'
|| chr(10) || ' c4_name as "Category"'
|| chr(10) || ' from issues'
|| chr(10) || ' left outer join users cr8 on cr8.us_id = i_created_by_user'
|| chr(10) || ' left outer join custom_1 on c1_id = i_custom_1'
|| chr(10) || ' left outer join custom_2 on c2_id = i_custom_2'
|| chr(10) || ' left outer join custom_4 on c4_id = i_custom_4'
|| chr(10) || ' where i_assigned_to_user = $ME and i_custom_1 != 3'
|| chr(10) || ' order by i_id desc');




/* created by me */
insert into queries (qu_name, qu_description, qu_sql) values (
'Open issues created by me',

'Notice the "$ME" in the query. The app replaces that with the id of the logged in user.'
|| chr(10) || ' Here we are using it to show only the issues created by the logged in user.',

'select i_id as "ID", i_description as "DESC", c1_name as "Status", asg.us_username as "Assigned To",' 
|| chr(10) || ' c2_name as "Priority",'
|| chr(10) || ' lu.us_username as "Last Updated By",'
|| chr(10) || ' i_last_updated_date as "Last Update",'
|| chr(10) || ' lp.us_username as "Last Post By",'
|| chr(10) || ' i_last_post_date as "Last Post"'
|| chr(10) || ' from issues'
|| chr(10) || ' left outer join users asg on asg.us_id = i_assigned_to_user'
|| chr(10) || ' left outer join users lu on lu.us_id = i_last_updated_user'
|| chr(10) || ' left outer join users lp on lp.us_id = i_last_post_user'
|| chr(10) || ' left outer join custom_1 on c1_id = i_custom_1'
|| chr(10) || ' left outer join custom_2 on c2_id = i_custom_2'
|| chr(10) || ' where i_created_by_user = $ME and i_custom_1 != 3'
|| chr(10) || ' order by i_id desc');


/* my org */

insert into queries (qu_name, qu_description, qu_sql) values (
'My Organization''s issues',

'Notice the "$ME" in the query. The app replaces that with the id of the logged in user.'
|| chr(10) || ' Here we are using it to show only the issues associated with the logged in user''s org.',

'select i_id as "ID", i_description as "DESC", c1_name as "Status", asg.us_username as "Assigned To",' 
|| chr(10) || ' c2_name as "Priority",'
|| chr(10) || ' c4_name as "Category"'
|| chr(10) || ' from issues'
|| chr(10) || ' inner join organizations on og_id = i_organization'
|| chr(10) || ' inner join users me on me.us_id = $ME'
|| chr(10) || ' left outer join users asg on asg.us_id = i_assigned_to_user'
|| chr(10) || ' left outer join custom_1 on c1_id = i_custom_1'
|| chr(10) || ' left outer join custom_2 on c2_id = i_custom_2'
|| chr(10) || ' left outer join custom_4 on c4_id = i_custom_4'
|| chr(10) || ' where i_organization = me.us_organization'
|| chr(10) || ' order by i_id desc');


/* latest activity */

insert into queries (qu_name, qu_description, qu_sql) values (
'Most recently active issues',

'Compare which is the most recent date, created, updated, or post',

'select i_id as "ID", i_description as "DESC", c1_name as "Status", asg.us_username as "Assigned To",' 
|| chr(10) || ' case'
|| chr(10) || ' when i_last_post_date    > i_created_date and i_last_post_date    > coalesce(i_last_updated_date, ''1999-01-01'') then i_last_post_date' 
|| chr(10) || ' when i_last_updated_date > i_created_date and i_last_updated_date > coalesce(i_last_post_date,    ''1999-01-01'') then i_last_updated_date' 
|| chr(10) || ' else i_created_date'
|| chr(10) || ' end as "Latest Activity Date",'
|| chr(10) || ' case'
|| chr(10) || ' when i_last_post_date    > i_created_date and i_last_post_date    > coalesce(i_last_updated_date, ''1999-01-01'') then ''Posted to''' 
|| chr(10) || ' when i_last_updated_date > i_created_date and i_last_updated_date > coalesce(i_last_post_date,    ''1999-01-01'') then ''Updated''' 
|| chr(10) || ' else ''Created'''
|| chr(10) || ' end as "Latest Activity",'
|| chr(10) || ' case'
|| chr(10) || ' when i_last_post_date    > i_created_date and i_last_post_date    > coalesce(i_last_updated_date, ''1999-01-01'') then posted.us_username' 
|| chr(10) || ' when i_last_updated_date > i_created_date and i_last_updated_date > coalesce(i_last_post_date,    ''1999-01-01'') then updated.us_username'
|| chr(10) || ' else created.us_username'
|| chr(10) || ' end as "Latest Activity By"'
|| chr(10) || ' from issues'
|| chr(10) || ' inner join users created on created.us_id = i_created_by_user'
|| chr(10) || ' left outer join users asg on asg.us_id = i_assigned_to_user'
|| chr(10) || ' left outer join custom_1 on c1_id = i_custom_1'
|| chr(10) || ' left outer join users updated on updated.us_id = i_last_updated_user'
|| chr(10) || ' left outer join users posted on posted.us_id = i_last_post_user'
|| chr(10) || ' order by "Latest Activity Date" desc');


/* attachments */
insert into queries (qu_name, qu_description, qu_sql) values (
'Issues with largest file attachments',

'Note that if an issue has multiple attachments, it will show up in multiple rows.',

'select i_id as "ID", i_description as "Description",'
|| chr(10) || ' us_username as "Uploaded By", p_created_date as "Upload Date",'
|| chr(10) || ' pa_file_name as "File Name", pa_file_content_type as "File Type", pa_file_length as "File Size"'
|| chr(10) || ' from post_attachments'
|| chr(10) || ' inner join posts on pa_post = p_id'
|| chr(10) || ' inner join issues on pa_issue = i_id'
|| chr(10) || ' inner join users on us_id = p_created_by_user'
|| chr(10) || ' order by pa_file_length desc');

/* for testing logic that inserts org permission into queries */
insert into queries (qu_name, qu_sql) values('Test 1: /*AND_ORG*/',
'/*TEST1*/ select i_id, i_description, i_organization, i_custom_1 from issues where i_custom_1 != 0 /*AND_ORG*/ order by i_id desc'
);

insert into queries (qu_name, qu_sql) values('Test 2: /*WHERE_ORG*/',
'/*TEST2*/ select i_id, i_description, i_organization, i_custom_1 from issues /*WHERE_ORG*/ order by i_id desc'
);

insert into queries (qu_name, qu_sql) values('Test 3: where and order ',
'/*TEST3*/ select i_id, i_description, i_organization, i_custom_1 from issues where i_custom_1 != 0 order by i_id desc'
);

insert into queries (qu_name, qu_sql) values('Test 4: where and no order',
'/*TEST4*/ select i_id, i_description, i_organization, i_custom_1 from issues where i_custom_1 != 0'
);

insert into queries (qu_name, qu_sql) values('Test 5: no where and order',
'/*TEST5*/ select i_id, i_description, i_organization, i_custom_1 from issues order by i_id desc'
);

insert into queries (qu_name, qu_sql) values('Test 6: no where and no order',
'/*TEST6*/ select i_id, i_description, i_organization, i_custom_1 from issues'
);

       