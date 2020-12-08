# Invoke the deploy script on the remote server

# make sure budoco_active_config.txt is in remote budoco folder

scp deploy_to_production.sh root@157.230.222.170:/root/budoco
ssh root@157.230.222.170  'chmod +x budoco/deploy_to_production.sh'
ssh root@157.230.222.170  budoco/deploy_to_production.sh