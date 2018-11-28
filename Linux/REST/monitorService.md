
```bash
curl -s -o /dev/null -I -w "%{http_code}" http://www.example.org/
```
-s = Don't show download progress, -o /dev/null = don't display the body, -w "%{http_code}"= Write http response code to stdout after exit.

monitor.sh
```bash
#!/bin/bash
DATETIME=$(date -u --iso-8601=s)
HTTP_STATUS=$(curl -s -o /dev/null -I -w "%{http_code}" http://services.dc1.ovid.com/QueryMapping/version)
echo "$DATETIME $HTTP_STATUS" >> monitoring.log
```

To run it periodically (each 5 s)
```bash
watch -n 5 ./monitor.sh
```

Run as a daemon
```bash
nohup watch -n 5 ./monitor.sh &
```