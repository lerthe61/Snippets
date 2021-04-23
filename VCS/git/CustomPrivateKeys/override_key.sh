# Override the key to be used with the current repo

# the following script has to be run under repository folder
if [ -d ".git" ]; then
  echo "No git repository found."
  exit 1
fi