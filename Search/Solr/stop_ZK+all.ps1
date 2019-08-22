# Set SOLR_HOME
$env:SOLR_HOME="C:\Java\solr-6.3.0p"

# Stop Solr
& "$env:SOLR_HOME\bin\solr.cmd" stop -all

# If we have a zookeeper job stop that also
if ($Global:zkJob) {
	"Stopping zookeeper"
	Stop-Job $zkJob
	Remove-Job $zkJob
}