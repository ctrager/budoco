
drop table if exists users;
drop table if exists sessions;
drop table if exists issues;
drop table if exists projects;
drop table if exists categories;
drop table if exists priorities;
drop table if exists statuses;
drop table if exists queries;
drop table if exists posts;
drop table if exists emailed_links;

create table users
(
us_id serial,
us_username varchar(20) not null,
us_email varchar(40) not null,
us_is_admin boolean default false,
us_password varchar(48) not null default '',
us_is_active boolean default true,
us_create_timestamp timestamptz default CURRENT_TIMESTAMP not null
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
pj_default int not null default 0,
pj_sort_seq int not null default 0
);

CREATE UNIQUE INDEX pj_name_index ON projects (pj_name);

create table categories
(
ca_id serial,
ca_name varchar(30) not null,
ca_default int not null default 0,
ca_sort_seq int not null default 0
);

CREATE UNIQUE INDEX ca_name_index ON categories (ca_name);

create table statuses
(
st_id serial,
st_name varchar(30) not null,
st_default int not null default 0,
st_sort_seq int not null default 0
);

CREATE UNIQUE INDEX st_name_index ON statuses (st_name);

create table priorities
(
pr_id serial,
pr_name varchar(30) not null,
pr_default int not null default 0,
pr_sort_seq int not null default 0
);

CREATE UNIQUE INDEX pr_name_index ON priorities (pr_name);

create table queries
(
qu_id serial,
qu_name varchar(60) not null,
qu_sql text not null,
qu_sort_seq int not null default 0
);

create unique index qu_name_index on queries (qu_name);

insert into users (us_username, us_email, us_is_admin, us_password) values('admin', '', true, '');
insert into users (us_username, us_email, us_is_admin, us_password) values('corey', 'ctrager@gmail.com', true, 'x');
insert into users (us_username, us_email, us_is_admin, us_password) values('misayo', 'm@example.com', false, 'x');
insert into users (us_username, us_email, us_is_admin, us_password) values('abi', 'a@example.com', false, 'x');
insert into users (us_username, us_email, us_is_admin, us_password) values('isaac', 'i@example.com', false, 'x');

insert into projects (pj_name) values ('proj 1');
insert into projects (pj_name) values ('proj 2');
insert into projects (pj_name) values ('proj 3');

insert into categories (ca_name) values ('bug');
insert into categories (ca_name) values ('task');
insert into categories (ca_name) values ('question');

insert into priorities (pr_name) values ('1-high');
insert into priorities (pr_name) values ('2-medium');
insert into priorities (pr_name) values ('3-low');

insert into statuses (st_name, st_default) values ('new', 1);
insert into statuses (st_name) values ('in progress');
insert into statuses (st_name) values ('done');

create table issues 
(
i_id serial,
i_desc varchar(200) not null,
i_created_by_user int not null default 0,
i_created_date timestamptz default CURRENT_TIMESTAMP,
i_status int not null default 0,
i_priority int not null default 0,
i_category int not null default 0,
i_project int not null default 0,
i_assigned_to_user int null default 0,
i_last_updated_user int null default 0,
i_last_updated_date timestamptz null
);



create table posts 
(
p_id serial,
p_issue int,
p_text text,
p_created_by_user int not null,
p_created_date timestamptz default CURRENT_TIMESTAMP
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
|| chr(10) || ' where (i_assigned_to_user_id == $ME) '
|| chr(10) || ' order by i_id desc',
2);

