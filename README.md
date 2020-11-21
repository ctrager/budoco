# Budoco

## What is Budoco?

Budoco is (so far, Nov 2020) a minimalist rewrite of **Bu**gTracker.NET on **Do**tnet **Co**re.

BugTracker.NET is an bug/issue tracking web app that runs on ASP.NET and uses MS SQL Server. More about BugTracker.NET here: <a href="http://ifdefined.com/bugtrackernet.html">BugTracker.NET home page</a>

I first released BugTracker.NET in 2001, originally to learn the hot new language C#. Within a couple years BugTracker.NET was pretty solid. Probably thousands of organziations used it and maybe many are still using it.

But, time moves on.

I moved on to other technologies, and then retired and stopped coding in 2016, 
Dev teams moved on to Github and Jira for their issue tracking needs.
Microsoft moved on too.

But then...2020: I started writing Budoco in November 2020 in order to distract myself from election anxiety and to break the COVID-19 monotony.

Budoco runs on Microsoft's cross-platform Dotnet Core 5 can run. I've been developing it on Linux Mint, based on Ubuntu 20.04. It uses Postgres as its database.

If Budoco is amusing you in any way, let me know at ctrager@yahoo.com
   


## How to Install


dotnet install package Serilog.AspNetCore

to debug and auto load,
run dotnet watch run in terminal, attach to budoco process

dotnet install package mailkit

sudo apt install postgresql

edit /etc/posgres/

/etc/postgresql/12/main/pg_hba.confi

change postgres method from "peer" to "md5"

sudo -u postgres psql

alter user postgres PASSWORD 'MY PASSWORD';

systemctl restart postgresql

psql -d budoco -U postgres -f misc/setup.sql


## Corey's TODO:

These instructions are a work in progress. They are for Linux Mint 20, based on Ubuntu 20.04:

1) Install Posgres

2) Install dotnet core sdk

3) Edit appsettings.json and/or appsettings.Develop.json

4) Create your two files to hold your postgres and smtp passwords. The files should contain just one line, your password, with no line break.

5) Build and run:
    
dotnet build



deploy/install, both google and yahoo require extra steps to enable smtp

issue - text
issue - posts
issue - attachments (blobs)

full text search

more query examples

create and run queries

admin pages
