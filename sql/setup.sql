
drop table if exists users;
drop table if exists sessions;
drop table if exists registration_requests;
drop table if exists reset_password_requests;
drop table if exists outgoing_email_queue;

drop table if exists issues;
drop table if exists posts;
drop table if exists post_attachments;

drop table if exists organizations;
drop table if exists queries;
drop table if exists custom_1;
drop table if exists custom_2;
drop table if exists custom_3;
drop table if exists custom_4;
drop table if exists custom_5;
drop table if exists custom_6;

drop table if exists reports;


create table users
(
us_id serial primary key,
us_username varchar(20) not null,
us_email_address varchar(64) not null,
us_password varchar(48) not null default '',
us_is_admin boolean default false,
us_is_active boolean default true,
us_is_report_only boolean default false,
us_organization int not null default 0,
us_created_date timestamptz default CURRENT_TIMESTAMP not null
);

CREATE UNIQUE INDEX us_username_index ON users (us_username);
CREATE UNIQUE INDEX us_email_address_index ON users (us_email_address);

create table sessions
(
	se_id varchar(40) not null,
	se_timestamp timestamptz default CURRENT_TIMESTAMP not null,
	se_user int not null
);

CREATE UNIQUE INDEX se_id_index ON sessions (se_id);


create table organizations
(
og_id serial primary key,
og_name varchar(30) not null,
og_is_active boolean default true,
og_is_default boolean default false
);

CREATE UNIQUE INDEX og_name_index ON organizations (og_name);

create table custom_1
(
c1_id serial primary key,
c1_name varchar(30) not null,
c1_is_active boolean default true,
c1_is_default boolean default false
);

CREATE UNIQUE INDEX c1_name_index ON custom_1 (c1_name);

create table custom_2
(
c2_id serial primary key,
c2_name varchar(30) not null,
c2_is_active boolean default true,
c2_is_default boolean default false
);

CREATE UNIQUE INDEX c2_name_index ON custom_2 (c2_name);

create table custom_3
(
c3_id serial primary key,
c3_name varchar(30) not null,
c3_is_active boolean default true,
c3_is_default boolean default false
);

CREATE UNIQUE INDEX c3_name_index ON custom_3 (c3_name);

create table custom_4
(
c4_id serial primary key,
c4_name varchar(30) not null,
c4_is_active boolean default true,
c4_is_default boolean default false
);

CREATE UNIQUE INDEX c4_name_index ON custom_4 (c4_name);

create table custom_5
(
c5_id serial primary key,
c5_name varchar(30) not null,
c5_is_active boolean default true,
c5_is_default boolean default false
);

CREATE UNIQUE INDEX c5_name_index ON custom_5 (c5_name);

create table custom_6
(
c6_id serial primary key,
c6_name varchar(30) not null,
c6_is_active boolean default true,
c6_is_default boolean default false
);

CREATE UNIQUE INDEX c6_name_index ON custom_6 (c6_name);

create table queries
(
qu_id serial primary key,
qu_name varchar(80) not null,
qu_sql text not null,
qu_description text not null default '',
qu_is_active boolean default true,
qu_is_default boolean default false
);

create unique index qu_name_index on queries (qu_name);

/* REPORTS */
create table reports
(
	rp_id serial primary key,
	rp_name varchar(80) not null,
	rp_sql text not null,
	rp_chart_type varchar(8) not null
);

create unique index unique_rp_name on reports (rp_name);


create table issues 
(
i_id serial primary key,
i_description varchar(200) not null,
i_details text not null default '',
i_created_by_user int not null default 0,
i_created_date timestamptz default CURRENT_TIMESTAMP,
i_organization int not null default 0,
i_custom_1 int null default 0,
i_custom_2 int null default 0,
i_custom_3 int null default 0,
i_custom_4 int null default 0,
i_custom_5 int null default 0,
i_custom_6 int null default 0,
i_assigned_to_user int null default 0,
i_last_updated_user int null default 0,
i_last_updated_date timestamptz null,
i_last_post_user int null default 0,
i_last_post_date timestamptz null

);

create table posts 
(
p_id serial primary key,
p_issue int not null,
/* comment, email, reply, history */
p_post_type varchar(12) not null,
p_text text not null default '',
p_email_to text not null default '',
p_email_from text not null default '',
p_email_from_html_part text not null default '',
p_changed_field varchar(30) not null default '',
p_before_val text not null default '',
p_after_val text not null default '',
p_created_by_user int not null,
p_created_date timestamptz default CURRENT_TIMESTAMP
);

create index p_issue_index on posts (p_issue);

create table post_attachments
(
pa_id serial primary key,
pa_post int not null,
pa_issue int not null, 
pa_file_name text not null default '',
pa_file_length int not null default 0,
pa_file_content_type text not null default '',
pa_content bytea null
);

create index pa_post_index on post_attachments (pa_post);

create table registration_requests
(
rr_id serial primary key,
rr_guid varchar(36) not null,
rr_created_date timestamptz default CURRENT_TIMESTAMP,
rr_email_address varchar(64) not null,
rr_username varchar(20) not null,
rr_password varchar(48) not null default '',
rr_is_invitation bool not null default false,
rr_organization int not null default 0
);

create unique index rr_guid_index on registration_requests (rr_guid);

create table reset_password_requests
(
rp_guid varchar(36) not null,
rp_created_date timestamptz default CURRENT_TIMESTAMP,
rp_email_address varchar(64) not null,
rp_user_id int not null
);

create unique index rp_guid_index on reset_password_requests (rp_guid);

create table outgoing_email_queue 
(
oq_id serial primary key,
oq_date_created timestamptz default CURRENT_TIMESTAMP,
oq_email_type varchar(10) not null, /* post, registration, forgot password */
oq_post_id int null, /* if related to post - get the attachments from it, don't store twice */
oq_sending_attempt_count int not null default 0,
oq_last_sending_attempt_date timestamptz null,
oq_last_exception text not null default '',
oq_email_to text not null,
oq_email_subject text not null,
oq_email_body text not null
);

create index oq_id_index on outgoing_email_queue (oq_date_created);

/* built in user, for when the system adds issues and posts */
insert into users (us_username, us_email_address, us_is_active) 
values('system', 'dummy', false);

/* you start of with this user, but you can add other admins 
and deactiviate this */
insert into users (us_username, us_email_address, us_is_admin) 
values('admin', 'admin@example.com', true);

insert into organizations (og_name) values ('org 1');
insert into organizations (og_name) values ('org 2');
insert into organizations (og_name) values ('org 3');

insert into custom_1 (c1_name, c1_is_default) values ('New', true);
insert into custom_1 (c1_name) values ('In Progress');
insert into custom_1 (c1_name) values ('Done');

insert into custom_2 (c2_name) values ('1 High');
insert into custom_2 (c2_name, c2_is_default) values ('2 Medium', true);
insert into custom_2 (c2_name) values ('3 Low');

insert into custom_3 (c3_name) values ('Red');
insert into custom_3 (c3_name) values ('Blue');
insert into custom_3 (c3_name) values ('Green');

insert into custom_4 (c4_name) values ('Bug');
insert into custom_4 (c4_name) values ('Task');
insert into custom_4 (c4_name) values ('Question');

insert into queries (qu_name, qu_sql) values (
'Raw "select * from issues" Please run queries.sql',
'select * from issues order by i_id desc');

-- one row, for schema changes, "migrations"
drop table db_version;
create table db_version
(
db_version int 
);
insert into db_version (db_version) values(4);
