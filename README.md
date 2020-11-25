## Budoco

## What is Budoco?

Budoco is a Issue/Bug/Task tracking system.

Budoco is a lighter weight rewrite of **Bu**gTracker.NET, but this time on **Do**tnet **Co**re. 

Budoco is cross platform and uses PostgreSQL. BugTracker.NET runs only on Windows and uses MS Sql Server.

More info about BugTracker.NET here: <a href="http://ifdefined.com/bugtrackernet.html">BugTracker.NET home page</a>

I first released BugTracker.NET in 2001, originally to learn the hot new language C#. Within a couple years BugTracker.NET was pretty solid. Probably thousands of organziations used it and maybe many are still using it. But, time moves on. I moved on to other technologies, and then retired and mostly stopped coding in 2016. Dev teams moved on too, to Github and Jira for their issue tracking needs. Microsoft moved on too, from Windows only .NET to cross-platform Dotnet Core. They are no longer developing the technologies BugTracker.NET depends on.

If Budoco is amusing you in any way, let me know at ctrager@yahoo.com
  
## How to Install

These instructions worked for me using Linux Mint 20 based on Ubuntu 20.04.

### 1) Install dotnet core 5 sdk

Skip to the next step if you already have dotnet core 5 sdk installed.

I used the instructions here in Nov 2020: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update 

sudo apt-get install -y apt-transport-https
sudo apt-get update 
sudo apt-get install -y dotnet-sdk-5.0
```


### 2) Install PostgreSQL 12

These were the steps I followed to install postgresql and tweak it. I have no idea if this is best practice or something really bad, but it worked for me.

```
sudo apt install postgresql
```

Then use the psql command line tool to give the postgres user a password.

```
sudo -u postgres psql
alter user postgres password 'YOUR PASSWORD';
\q

```
Then change postgres configuration to require password:
```
sudo nano /etc/postgresql/12/main/pg_hba.conf
```
Change the line below so from "peer" to "md5"
```
local     all     postgres     peer
```
Restart postgres
```
systemctl restart postgresql
```
The following should now prompt for a password:
```
psql -U postgres
```
To avoid being prompted every time, create this file in your home dir:
```
.pgpass
```
with one line:
```
localhost:5432:*:postgres:YOUR PASSWORD
```


### 3) Create the Budoco database.

Create the database:

```
psql -U postgres
create database budoco;
\q
```

Create the tables:

```
psql -d budoco -U postgres -f sql/setup.sql
psql -d budoco -U postgres -f sql/queries.sql
```

### 4) Configure Budoco

Copy the example config file. The copy must be named "budoco_config_active.txt"

```
cp budoco_config_example.txt budoco_config_active.txt
```

Open the new copy and edit it according to the instructions in it. 
The hardest part is getting your emails to work, not just because of getting the settings correct but because Gmail, Yahoo, etc, make you do extra steps at their website, for everybody's protection.

\<RANT>
Here we are in the year 2020 and Microsoft adopted a format for configuration files, "appsetings.json", that does *NOT* support comments. It makes me miss the ".ini" files from Windows 3.1 from the early 90s.
\</RANT>

## Running Budoco

```
dotnet run
```

Login as admin/admin. You will be redirected to a reset password page. If you are just playing around and want to create more users, set "DebugAutoConfirmRegistration" to 1 in budoco_config_active.txt.

## Using Budoco

The philosophy of both old BugTracker.NET and new Budoco is that they are easy to get started with but if you want to, you can customize it by writing SQL queries and then storing those queries in the database. The Issues page displays the list of queries in its dropdown. 

Note the little variable $ME in some of the queries, which you can use like this to limit rows to just that user's issues:
```
where i_created_by_user = $ME
```
However there isn't any per-issue permission system yet. Although your query limits what the user can see in the list, any user can see any issue by just typing in the issue number into the "Go To" box at the top. The only permission system so far is that you can designate a user as report-only or view-only.

## Corey's Roadmap/TODO:

* Revisit/retest view-only and report-only users.

* Finish admin pages

* Track last post user/date and status change user/date as fields on issue. It's not a notification system but it's a decent way of seeing what has most recently changed.

* More query examples in queries.sql.

* Explore jquery menu widget, because I hate that extra click to open the dropdown.

* Send emails from the Issue page, that become part of the Issue posts.

* *RECEIVE* emails into the app that get posted to the relevant Issue.


### Features that I'll probably never work on, because they are not fun.

* More and more permissions. It's fun to write code that allows the user to do something. It's less fun to write code to STOP the user from doing something. If you reall
  
* Any sort of enforcement of workflow.

* Custom fields. 
  
* Integration with version control. If you want that, just use Github, etc.

* Email/Slack notifications about changes, or reminders that a issue is growing stale. This app is not going to nag you.

