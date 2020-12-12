BEGIN;

create table junk1
(
my_col int
);

update db_version set db_version = 2;


COMMIT;