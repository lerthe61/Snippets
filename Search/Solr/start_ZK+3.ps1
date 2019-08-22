# Set ZOOKEEPER_HOME
$env:ZOOKEEPER_HOME="C:\Java\zookeeper-3.4.6"

# Start zookeeper in background
"Starting zookeeper"
$Global:zkJob = Start-Job -Name zooKpr -ScriptBlock {& "$env:ZOOKEEPER_HOME\bin\zkServer.cmd"}

# Set SOLR_HOME
$env:SOLR_HOME="C:\Java\solr-6.3.0p"

# Start 3 solr nodes on ports 8171, 8181, 8191
# First set the log directory, then start Solr
$env:SOLR_LOGS_DIR="C:\logs\solr\8171"
& "$env:SOLR_HOME\bin\solr.cmd" start -cloud -zkhost localhost:2181/solr -h $env:computername -p 8171 -s /data/solr/8171

$env:SOLR_LOGS_DIR="C:\logs\solr\8181"
& "$env:SOLR_HOME\bin\solr.cmd" start -cloud -zkhost localhost:2181/solr -h $env:computername -p 8181 -s /data/solr/8181

$env:SOLR_LOGS_DIR="C:\logs\solr\8191"
& "$env:SOLR_HOME\bin\solr.cmd" start -cloud -zkhost localhost:2181/solr -h $env:computername -p 8191 -s /data/solr/8191