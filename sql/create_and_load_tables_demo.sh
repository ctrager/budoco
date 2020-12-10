psql -d budoco -U postgres -f setup.sql
psql -d budoco -U postgres -f queries.sql
psql -d budoco -U postgres -f reports.sql
psql -d budoco -U postgres -f generate_test_data.sql

