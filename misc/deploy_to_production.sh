# scp this to home folder, for example /home/corey, or /root
# this is the script that executes ON the remote server

cd budoco

git pull
# so that we can look on the server and see what we are deploying
git log -3 > git_log.txt
dotnet publish -o ../publish_next
cp budoco_config_active.txt ../publish_next
cp git_log.txt ../publish_next
touch ../publish_next/$( date '+%Y-%m-%d_%H-%M-%S' )

# uncomment to reload db
#psql -d budoco -U postgres -f sql/setup.sql
#psql -d budoco -U postgres -f sql/queries.sql
#psql -d budoco -U postgres -f sql/reports.sql
#psql -d budoco -U postgres -f sql/generate_test_data.sql

cd ..

# swap folders

rm -rf publish_prev
mv publish publish_prev
mv publish_next publish
systemctl restart budoco
