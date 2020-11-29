# scp this to home folder, for example /home/corey, or /root

cd budoco

git pull
dotnet publish -o ../publish_next
cp budoco_config_active.txt ../publish_next
touch ../publish_next/$( date '+%Y-%m-%d_%H-%M-%S' )

cd ..

# swap, roll

rm -rf publish_prev
mv publish publish_prev
mv publish_next publish
systemctl restart budoco
