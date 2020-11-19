
drop table if exists users;

create table users
(
us_id serial,
us_username varchar(20) not null,
us_email varchar(40) not null,
us_is_admin boolean,
us_password varchar(32),
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


/*

us_salt int null,
us_password varchar(64) not null,
us_firstname nvarchar(60) null,
us_lastname nvarchar(60) null,
us_email nvarchar(120) null,
us_admin int not null default(0),
us_default_query int not null default(0),
us_enable_notifications int not null default(1),
us_auto_subscribe int not null default(0),
us_auto_subscribe_own_bugs int null default(0),
us_auto_subscribe_reported_bugs int null default(0),
us_send_notifications_to_self int null default(0),
us_active int not null default(1),
us_bugs_per_page int null,
us_forced_project int null,
us_reported_notifications int not null default(4),
us_assigned_notifications int not null default(4),
us_subscribed_notifications int not null default(4),
us_signature nvarchar(1000) null,
us_use_fckeditor int not null default(0),
us_created_user int not null default(1),
us_org int not null default(1),
us_most_recent_login_datetime datetime null
*/

insert into users (us_username, us_email, us_is_admin) values('admin', '', true);
insert into users (us_username, us_email, us_is_admin) values('corey', 'ctrager@gmail.com', false);
insert into users (us_username, us_email, us_is_admin) values('misayo', 'm@example.com', false);
insert into users (us_username, us_email, us_is_admin) values('abi', 'a@example.com', false);
insert into users (us_username, us_email, us_is_admin) values('isaac', 'i@example.com', false);

