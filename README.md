## Budoco

## What is Budoco?

Budoco is (so far, Nov 2020) a minimalist rewrite of **Bu**gTracker.NET on **Do**tnet **Co**re.

BugTracker.NET is an bug/issue tracking web app that runs on ASP.NET and uses MS SQL Server. More about BugTracker.NET here: <a href="http://ifdefined.com/bugtrackernet.html">BugTracker.NET home page</a>

I first released BugTracker.NET in 2001, originally to learn the hot new language C#. Within a couple years BugTracker.NET was pretty solid. Probably thousands of organziations used it and maybe many are still using it.

But, time moves on.

I moved on to other technologies, and then retired and stopped coding in 2016, 
Dev teams moved on to Github and Jira for their issue tracking needs.
Microsoft moved on too.

But then...2020: I started writing Budoco in November 2020 in order to distract myself from election anxiety and to break the COVID-19 monotony. 

Budoco runs on Microsoft's cross-platform Dotnet Core 5. I've been developing it on Linux Mint based on Ubuntu 20.04. It uses Postgres as its database.

If Budoco is amusing you in any way, let me know at ctrager@yahoo.com
   


## How to Install


These instructions are a work in progress. They are for Linux Mint 20, based on Ubuntu 20.04. 

### 1) Create the postgresql database. 

If you already have postgresql running and you have a postgres user and password combo that works, then just create an empty database for Budoco and skip ahead. I named my database "budoco".

These were the steps I followed to install postgresql and tweak it. I have no idea if this is best practice or something really bad, but it worked for me.

```
sudo apt install postgresql
```

Then start psql command line tool:

```
sudo -u postgres psql
```

```
alter user postgres password 'YOUR PASSWORD';
create database budoco;
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


### 2) Install dotnet core 5 sdk

Skip to step 3 if you already have the dotnet core 5 sdk installed.

I used the instructions here in Nov 2020: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update 

sudo apt-get install -y apt-transport-https
sudo apt-get update 
sudo apt-get install -y dotnet-sdk-5.0
```
### 3) Create the Budoco tables.

Assuming you've checked out this repository into a folder called budoco, navigate to that folder and create the tables:
```
psql -d budoco -U postgres -f misc/setup.sql
psql -d budoco -U postgres -f misc/queries.sql
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

Login as admin. Type something, anything as your password. You will be redirected to a reset password page. If you are just playing around and want to create more users, set "DebugAutoConfirmRegistration" to 1 in budoco_config_active.txt.

## Using Budoco

The philosophy of both old BugTracker.NET and new Budoco is that you customize it by writing SQL queries and then storing those queries in the database. The Issues page displays the list of queries in its dropdown.

Add your queries following the examples in queries.sql

Note the little variable $ME in some of the queries, which you can use like this to restrict rows to just the logged on users issues:
```
where i_created_by_user = $ME
```

## Corey's Roadmap/TODO:

* Search - postgres not working as documented, or me?
https://www.postgresql.org/docs/11/textsearch-controls.html

select i_id, i_description, 0 as p_id, '' as p_text, 

websearch_to_tsquery('english', 
'corey peanut'

),

ts_rank(to_tsvector('english', i_description), 
websearch_to_tsquery('english', 
'corey peanut'

)) as rank
from issues 

order by rank desc limit 20

* More query examples in queries.sql

* Finish admin pages

* Send emails from the Issue page, that become part of the Issue posts.
* *RECEIVE* emails into the app that get posted to the relevant Issue.

* Org permissions - maybe the only permissions the app needs.
if user is associated with org, then can only see bugs associated with his org.
this supports one company, many customers
works with user is inactive until admin activates
inject where clause into queries

* Deleting stuff. Logically deleting stuff. Setting to inactive. Need to redo issue dropdowns

### BugTracker.NET features that I'll probably never work on, because they are not fun.

* More and more permissions.
  
* Custom fields. 
  
* Integration with version control. 

* email notifications about changes 
could use most recently updated bug instead

  

