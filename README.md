## Budoco

Table of Contents

* [What is Budoco?](#what-is-budoco)
* [How to Install](#how-to-install)
* [Using Budoco](#using-budoco)

## What is Budoco?

Budoco is a Issue (Bug/Task/Ticket) tracking system.

Budoco is built using .NET 5 ("dotnet core") and PostgreSQL.

Budoco is a rewrite of **BU**gtracker.NET (aka "btnet"), but this time on **DO**tnet **CO**re, hence the name BU-DO-CO. Good things about Budoco which were also true of btnet:


* Budoco is easy to install and start using. 

* It's fast and lightweight. It won't slow you down.

* It is highly customizable if you are comfortable tweaking some SQL statements. It's not too hard because you will have some examples to follow. Here's one way that Budoco can look: https://budoco.net. You can register as a new user or sign on as "admin", password "admin". It's just demo data but please be nice.

* It sends and receives email which gets tracked with the issue. So a good fit for a help desk, or company that works with multiple clients.

* It has a lightweight permission system specifically for keeping external organizations separate from each other.

* It works well with a companion [screenshot app](https://github.com/ctrager/budoco_screenshot) that lets you take a screenshot and post it as an issue with just two clicks.

* If you decide to fork the code and change it, your learning curve will be short because I'm too impatient to learn fancy abstractions. Each page is pretty much get the input, read the database, throw up some HTML.

Differences from btnet:
* Btnet allows for more customization, at least for now. 
* Btnet has a more complicated permission system. Too comlicated to use without worry.
* Just in general, btnet tried to please everybody, whereas Budoco is just trying to please about half of you.
* Btnet only runs on Windows. Budoco runs wherever .NET 5 runs. I've been using Linux (Ubuntu).
* Btnet uses MS SQL Server. Budoco uses PostgreSQL.

 More about btnet here; <a href="http://ifdefined.com/bugtrackernet.html">BugTracker.NET home page</a>


I first released btnet in 2001, originally to learn the hot new language C#. Within a couple years BugTracker.NET was pretty solid. Probably thousands of organziations used it and maybe many are still using it. But, time moves on. I moved on to other technologies, and then retired and mostly stopped coding in 2016. Dev teams moved on too, to Github and Jira for their issue tracking needs. Microsoft moved on too, from Windows only .NET to cross-platform Dotnet Core. They are no longer developing the technologies btnet depends on. So, why did I decide to do a rewrite of btnet? Because it was November 2020 and I needed a distraction from the US presedential election and COVID-19, that's why. 

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

Also, for the "Reports" charts feature:

```
sudo apt-get install libgdiplus
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

Create and load the tables. If you want to just try out Budoco, then use the demo script:

```
cd sql
./create_and_load_tables_demo.sh
```

When you are ready to use it for real, use the production script:

```
./create_and_load_tables_production.sh
```

Both scripts start by destroying whatever data is currently in the budoco database.

### 4) Configure Budoco

Copy the example config file. The copy must be named "budoco_config_active.txt"

```
cp budoco_config_example.txt budoco_config_active.txt
```

Open the new copy and edit it according to the instructions in it. At a minimum, change the database username and password. The hardest part for me was getting my emails to work using my gmail and yahoo accounts, because there are extra steps you have to do on their websites to allow unfamiliar apps to connect to their SMTP servers.

\<RANT>
Here we are in the year 2020 and Microsoft adopted a format for configuration files, "appsettings.json", that does *NOT* support comments. It makes me miss the ".ini" files from Windows 3.1 from the early 90s.
\</RANT>

## Running Budoco

For testing:

```
dotnet run
```

Login as admin/admin. You will be redirected to a reset password page. If you are just playing around and want to create more users, set "DebugAutoConfirmRegistration" to 1 in budoco_config_active.txt to SKIP the step where a user would have to confirm registration by clicking on a link in an email. Change the setting back when you are running it for real.

For production I recommend that you use budoco with nginx. (Corey TODO: explain this more)

## Using Budoco

The philosophy of both old BugTracker.NET and new Budoco is that they are easy to get started with but highly customizable. To get to know Buduco first try running it loaded with demo data (Corey TODO explain how to do this) and then take a tour. This should just take a few minutes:

* Read the comments in budoco_config_example.txt to get an overview of the customizations controlled via config. Experiment with some of the customization settings.

* Visit the "Issues" page and try the different views.

* Visit the "Admin" page and then the Admin "Issue Queries" page and see how the SQL implements the Issue views.

* Visit the Users and Organizations pages to get to know the permissions system.

Good luck, and let me know how it goes.

[Corey Trager](http://ctrager.github.io)

