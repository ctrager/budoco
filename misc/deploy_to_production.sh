# scp this to home folder, for example /home/corey, or /root
# this is the script that executes ON the remote server

cd budoco

git pull
dotnet publish -o ../publish_next
cp budoco_config_active.txt ../publish_next
touch ../publish_next/$( date '+%Y-%m-%d_%H-%M-%S' )
#psql -d budoco -U postgres -f sql/setup.sql
#psql -d budoco -U postgres -f sql/queries.sql
#psql -d budoco -U postgres -f sql/generate_test_data.sql
cd ..

# swap, roll

rm -rf publish_prev
mv publish publish_prev
mv publish_next publish
systemctl restart budoco
