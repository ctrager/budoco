## Budoco

## What is Budoco?

Budoco is a Issue/Bug/Task tracking system.

Budoco is a rewrite of **BU**gtracker.NET, but this time on **DO**tnet **CO**re. 

Budoco is cross platform and uses PostgreSQL. The old BugTracker.NET runs only on Windows and uses MS Sql Server.

More info about BugTracker.NET here: <a href="http://ifdefined.com/bugtrackernet.html">BugTracker.NET home page</a>

I first released BugTracker.NET in 2001, originally to learn the hot new language C#. Within a couple years BugTracker.NET was pretty solid. Probably thousands of organziations used it and maybe many are still using it. But, time moves on. I moved on to other technologies, and then retired and mostly stopped coding in 2016. Dev teams moved on too, to Github and Jira for their issue tracking needs. Microsoft moved on too, from Windows only .NET to cross-platform Dotnet Core. They are no longer developing the technologies BugTracker.NET depends on. So, why did I decide to do a rewrite of BugTracker.NET? Because it was November 2020 and I needed a distraction from the US presedential election and COVID-19, that's why. 

If Budoco is amusing you in any way, let me know at ctrager@yahoo.com
  
## How to Install

These instructions worked for me using Linux Mint 20 based on Ubuntu 20.04, but Budoco should run anywhere dotnet core and postgres run.

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

Skip to the next step if you already have postgres installed and a username/password combo ready for Budoco.

These were the steps I followed to install postgres and tweak it. I have no idea if this is best practice but it worked for me.

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

Open the new copy and edit it according to the instructions in it. At a minimum, change the database username and password. The hardest part for me was getting my emails to work using my gmail and yahoo accounts, because there are extra steps you have to do on their websites to allow unfamiliar apps to connect to their SMTP servers.

\<RANT>
Here we are in the year 2020 and Microsoft adopted a format for configuration files, "appsetings.json", that does *NOT* support comments. It makes me miss the ".ini" files from Windows 3.1 from the early 90s.
\</RANT>

## Running Budoco

```
dotnet run
```

Login as admin/admin. You will be redirected to a reset password page. If you are just playing around and want to create more users, set "DebugAutoConfirmRegistration" to 1 in budoco_config_active.txt.

## Using Budoco

The philosophy of both old BugTracker.NET and new Budoco is that they are easy to get started with but highly customizable. Note the different views in the Issues page. Visit the Admin/Queries page. Try creating your own SQL views.  

Note the little variable $ME in some of the queries, which you can use like this to limit rows to just that user's issues:
```
where i_created_by_user = $ME
```

However there isn't any per-issue permission system yet. Although your query limits what the user can see in the list, any user can see any issue by just typing in the issue number into the "Go To" box at the top. The only permission system so far is that you can designate a user as report-only or view-only.

## Corey's Roadmap/TODO for winter 2020/2021:

* What's the "fakepath" thing in file picker, jquery, with attachments?

* Handle restart when user page is up, which cases session cached stuff to be lost

* when we are forcing admin to change password, also need email, in case forgets password
so need a new form

* *RECEIVE* emails into the app that get posted to the relevant Issue.

* Turn emails from trusted senders into issues.


* linkify urls in issue/post description


* Handle inactive options, add to dropdowns if issue being viewed
uses an inactive option

* make sure registration doesn't collide with user - need inactive user placeholder

### Features that I'll only get to if there are users out there willing to donate some money for them.

* More permissions/restrictions/workflow logic. It's fun to write code that allows the user to do something. It's less fun to write code to STOP the user from doing something. With the old BugTracker.NET, this kind of code became the hardest for me to work with and to be sure it was right.
 
* Custom fields. Do you **really** need them? 
  
* Integration with version control. If you want that, just use Github, etc.

* Email/Slack notifications about changes, or reminders that a issue is growing stale. 

url parser
https://mathiasbynens.be/demo/url-regex
https://gist.github.com/dperini/729294
Copyright (c) 2010-2018 Diego Perini (http://www.iport.it)