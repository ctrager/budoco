# run the deploy script

# make sure budoco_active_config.txt is in remote budoco folder

scp deploy_to_production.sh root@157.230.222.170:/
ssh root@157.230.222.170  ./deploy_to_production.sh