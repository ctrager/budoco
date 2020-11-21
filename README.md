# budoco
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


todo:

deploy/install, both google and yahoo require extra steps to enable smtp

issue - text
issue - posts
issue - attachments (blobs)

full text search

more query examples

create and run queries

admin pages
