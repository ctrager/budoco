
drop table if exists users;
drop table if exists sessions;
drop table if exists issues;
drop table if exists projects;
drop table if exists categories;
drop table if exists priorities;
drop table if exists statuses;
drop table if exists organizations;
drop table if exists queries;
drop table if exists posts;
drop table if exists post_attachments;
drop table if exists emailed_links;

create table users
(
us_id serial,
us_username varchar(20) not null,
us_email varchar(40) not null,
us_password varchar(48) not null default '',
us_is_admin boolean default false,
us_is_active boolean default true,
us_is_report_only boolean default false,
us_organization int not null default 0,
us_created_date timestamptz default CURRENT_TIMESTAMP not null
);

CREATE UNIQUE INDEX us_username_index ON users (us_username);
CREATE UNIQUE INDEX us_email_index ON users (us_email);

create table sessions
(
	se_id varchar(40) not null,
	se_timestamp timestamptz default CURRENT_TIMESTAMP not null,
	se_user int not null
);

CREATE UNIQUE INDEX se_id_index ON sessions (se_id);

create table projects
(
pj_id serial,
pj_name varchar(30) not null,
pj_is_active boolean default true,
pj_is_default boolean default false
);

CREATE UNIQUE INDEX pj_name_index ON projects (pj_name);

create table categories
(
ca_id serial,
ca_name varchar(30) not null,
ca_is_active boolean default true,
ca_is_default boolean default false
);

CREATE UNIQUE INDEX ca_name_index ON categories (ca_name);

create table statuses
(
st_id serial,
st_name varchar(30) not null,
st_is_active boolean default true,
st_is_default boolean default false
);

CREATE UNIQUE INDEX st_name_index ON statuses (st_name);

create table priorities
(
pr_id serial,
pr_name varchar(30) not null,
pr_is_active boolean default true,
pr_is_default boolean default false
);

CREATE UNIQUE INDEX pr_name_index ON priorities (pr_name);

create table organizations
(
og_id serial,
og_name varchar(30) not null,
og_is_active boolean default true,
og_is_default boolean default false
);

CREATE UNIQUE INDEX og_name_index ON organizations (og_name);

create table queries
(
qu_id serial,
qu_name varchar(60) not null,
qu_sql text not null,
qu_sort_seq int not null default 0
);

create unique index qu_name_index on queries (qu_name);

create table issues 
(
i_id serial,
i_description varchar(200) not null,
i_details text not null default '',
i_created_by_user int not null default 0,
i_created_date timestamptz default CURRENT_TIMESTAMP,
i_status int not null default 0,
i_priority int not null default 0,
i_category int not null default 0,
i_project int not null default 0,
i_organization int not null default 0,
i_assigned_to_user int null default 0,
i_last_updated_user int null default 0,
i_last_updated_date timestamptz null,
i_tsvector_for_search tsvector null
);

create table posts 
(
p_id serial,
p_issue int not null,
p_text text not null default '',
p_file_name text not null default '',
p_file_length int not null default 0,
p_file_content_type text not null default '',
p_email_to text not null default '',
p_email_from text not null default '',
p_email_subject text not null default '',
p_email_body text not null default '',
p_created_by_user int not null,
p_created_date timestamptz default CURRENT_TIMESTAMP,
p_tsvector_for_search tsvector null
);

create index p_issue_index on posts (p_issue);

create table emailed_links
(
el_guid varchar(36),
el_date timestamptz default CURRENT_TIMESTAMP,
el_email varchar(40) not null,
el_action varchar(20) not null, -- "registration" or "forgot"
el_username varchar(20) null,
el_user_id int null,
el_password varchar(48) null
);

create unique index el_guid_index on emailed_links (el_guid);

create table post_attachments
(
pa_id serial,
pa_post int not null,
pa_content bytea null
);

create unique index pa_post_index on post_attachments (pa_post);


insert into users (us_username, us_email, us_is_admin, us_password) 
values('admin', '', true, 'admin');

insert into projects (pj_name) values ('proj 1');
insert into projects (pj_name) values ('proj 2');
insert into projects (pj_name) values ('proj 3');

insert into categories (ca_name) values ('bug');
insert into categories (ca_name) values ('task');
insert into categories (ca_name) values ('question');

insert into priorities (pr_name) values ('1-high');
insert into priorities (pr_name) values ('2-medium');
insert into priorities (pr_name) values ('3-low');

insert into statuses (st_name, st_is_default) values ('new', true);
insert into statuses (st_name) values ('in progress');
insert into statuses (st_name) values ('done');

insert into organizations (og_name) values ('org 1');
insert into organizations (og_name) values ('org 2');
insert into organizations (og_name) values ('org 3');

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'Raw, no joins, no aliases',
'select * '
|| chr(10) || ' from issues '
|| chr(10) || ' order by i_id desc',
999);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'All issues',
'select i_id, i_description, i_project, i_organization, i_category, i_status, i_priority, i_created_by_user, i_assigned_to_user'
|| chr(10) || ' from issues '
|| chr(10) || ' order by i_id desc',
1);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'All open issues',
'select i_id as "ID", i_description as "Descripton", i_status as "Status", us_username as "Assigned to" '
|| chr(10) || ' from issues '
|| chr(10) || ' left outer join statuses on st_id = i_status'
|| chr(10) || ' left outer join users on us_id = i_assigned_to_user'
|| chr(10) || ' where (i_status != 3) '
|| chr(10) || ' order by i_id desc',
2);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'Issues assigned to me',
'select i_id as "ID", i_description as "Description", st_name as "Status"'
|| chr(10) || ' from issues '
|| chr(10) || ' left outer join statuses on st_id = i_status'
|| chr(10) || ' where (i_assigned_to_user = $ME) '
|| chr(10) || ' order by i_id desc',
2);

insert into queries (qu_name, qu_sql, qu_sort_seq) values (
'Many joins',
'select i_id as "ID", ' 
|| chr(10) || ' i_description as "DESC", pj_name as "Project", ca_name as "Category", cr8.us_username as "Created by",'
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

