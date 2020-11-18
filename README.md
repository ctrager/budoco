# net_razor
dotnet install package Serilog.AspNetCore

to debug and auto load,
run dotnet watch run in terminal, attach to net_razor process

dotnet install package mailkit

sudo apt install postgresql

edit /etc/posgres/

/etc/postgresql/12/main/pg_hba.confi

change postgres method from "peer" to "md5"
systemctl restart postgresql

sudo -u postgres psql

ALTER USER postgres PASSWORD 'myPassword';
ALTER ROLE