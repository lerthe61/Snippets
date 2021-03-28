#!/usr/bin/env bash
# dependency - jq utility, that can extract info from json
# Define your bitbucket server URL
BITBUCKET_SERVER=bitbucket.site.com
# Define your credentials
TOKEN=NTk2MjcyMjU3OTA1OpuuCANxT2fF+VNwQoeXs6PaAr87

# Save current dir
root_folder=$PWD

# for project in $(curl -s -H "Authorization: Bearer ${TOKEN}" --request GET https://$BITBUCKET_SERVER/rest/api/1.0/projects?limit=1000 | jq --raw-output '.values[].key')
# In this example we are using hardcoded value 'PS' as a reference to a single project. But porject can be obtained by utilizing th code from above.
for project in PS
do
  # Create project folder if it doesn't exist
  if [ ! -d ${project} ]; then
    echo -en " [\033[34mCreating project $project folder\033[0m]\n"
    mkdir $project
    echo -en " [\033[32mCreated\033[0m]\n"
  fi

  # Step into folder
  cd $project

  # Clone every repo in this project (http)
  for repo in $(curl -s -H "Authorization: Bearer ${TOKEN}" --request GET https://$BITBUCKET_SERVER/rest/api/1.0/projects/$project/repos?limit=1000 | jq --raw-output '.values[].links.clone[].href | select(. | contains("ssh"))')
  do
    echo -en " [\033[34mCloning $repo in $project\033[0m]\n"
    git clone $repo
    echo -en " [\033[32mCloned\033[0m]\n"
  done

  # Back to root folder
  cd $root_folder
done

echo -en " [\033[32mDone\033[0m]\n"

# OPTIONAL: Update all refs
#./update_git_repos.sh