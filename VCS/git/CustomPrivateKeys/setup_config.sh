#!/usr/bin/env bash
# Create a separate config for bitbucket.org with specified private key

# Setting the name for config
config_name="test"
config_location=~/.ssh/$config_name

# Setting the key name to use
key_name="lp_id_rsa"

# check if config already exist - exit with message
if [ -e $config_location ]; then
  echo "File $config_location already exists."
  exit 1
fi

# Creating and populating the config
cat > $config_location <<EOF
Host bitbucket.org
   HostName bitbucket.org
   Port 22   
   User git
IdentityFile ~/.ssh/$key_name
EOF
