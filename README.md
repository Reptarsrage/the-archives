# The Archives

Author: reptarsrage
Date: 08/28/2020  

![.NET](https://github.com/Reptarsrage/TheArchives/workflows/.NET/badge.svg)

An experiment with content aggregation and UI.

## Running locally

To Setup Elastic/Kibana:

```ps
docker-compose up -d
```

Then run the app using the IDE of your choice.

## Publishing

Simply run

```ps
./publish.ps1
```

> NOTE: Ensure you update the IP address of the server, and ensure youve set up SCP (see below).

## Setting up .NET on a Raspberry Pi

### Downloading .NET 5 SDK and Runtime

Head over to https://dotnet.microsoft.com/download/dotnet-core and download the latest ARM32 SDK and runtime packages.

```
wget https://download.visualstudio.microsoft.com/download/pr/fada9b0c-202a-4720-817b-b8b92dddad99/fa6ace43156b7f73e5f7fb3cdfb5c302/dotnet-sdk-5.0.202-linux-arm.tar.gz
wget https://download.visualstudio.microsoft.com/download/pr/254a9fbb-e834-470c-af08-294c274a349f/ee755caf0b8a801cf30dcdc0c9e4273d/aspnetcore-runtime-5.0.5-linux-arm.tar.gz
```

### Installing .NET 5 SDK and Runtime

Create a folder named for example `dotnet-arm32` and unzip them into it. 

```
mkdir dotnet-arm32
tar zxf dotnet-sdk-5.0.202-linux-arm.tar.gz -C $HOME/dotnet-arm32
tar zxf aspnetcore-runtime-5.0.5-linux-arm.tar.gz -C $HOME/dotnet-arm32
```

### Seting up the environment

Configure the `DOTNET_ROOT` and `PATH` environment variables for .NET CLI 

```
sudoedit ~/.profile
```

Add those lines at the end of this file

```
# set .NET Core SDK and Runtime path
export DOTNET_ROOT=$HOME/dotnet-arm32
export PATH=$PATH:$HOME/dotnet-arm32
```

### Seting up nginx

First, install and start Nginx:

```
sudo apt install nginx
sudo /etc/init.d/nginx start
```

Open Nginx config file:

```
sudoedit /etc/nginx/sites-available/default
```

Replace its content with:

```
server {
    listen        80 default_server;
    server_name   _;
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

Check and apply the config file:

```
sudo nginx -t
sudo nginx -s reload
```

### Seting up Kestral

We will create a systemd service.

```
sudoedit /etc/systemd/system/kestrel-thearchives.service
```

With the following content:

```
[Unit]
Description=.NET 6 App - The Archives

[Service]
WorkingDirectory=/var/www/the-archives/publish
ExecStart=/home/pi/dotnet-arm32/dotnet /var/www/the-archives/publish/TheArchives.Server.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-the-archives
User=pi

[Install]
WantedBy=multi-user.target
```

Note, we can only use absolute path in systemd configuation. 
Register and start the service:

```
sudo systemctl enable kestrel-thearchives.service
sudo systemctl start kestrel-thearchives.service
sudo systemctl status kestrel-thearchives.service
```

Or if the service already exists run:

```sh
sudo systemctl daemon-reload
```

To configure the necessary environment variables run `sudo systemctl edit kestrel-thearchives` and add the following overrides:

```
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=Auth0__Authority=https://dev-60pojokn.us.auth0.com
Environment=Auth0__ApiIdentifier=https://archives.dyndns.org
Environment="ConnectionStrings__Default=Data Source=/var/www/the-archives/Database.sqlite"
Environment=Elastic__Uri=http://localhost:9200/
Environment=Elastic__DefaultIndex=thearchives
Environment=Content__BaseDir=/var/www/the-archives/content/
Environment=LogDirectory=/var/www/the-archives/logs/
```

> NOTE: If : cannot be used in environment variables in your system, replace : with __ (double underscore). Any 
> variables with spaces must have double qoutes around the entire assignment.

> ALSO NOTE: Make sure to set up port forwarding on your router for ports 80 and 22.

## Installing Elasticsearch on Raspberry Pi

### Installing Java

Install Bellsoft OpenJRE 11:

```
wget https://download.bell-sw.com/java/11.0.9.1+1/bellsoft-jre11.0.9.1+1-linux-arm32-vfp-hflt.deb
sudo dpkg -i bellsoft-jre11.0.9.1+1-linux-arm32-vfp-hflt.deb
```

Copy the JRE into a directory that the elasticsearch installer will recognize:

```
sudo mkdir -p /usr/share/elasticsearch/jdk
sudo cp -rf /usr/lib/jvm/bellsoft-java11-runtime-arm32-vfp-hflt/* /usr/share/elasticsearch/jdk
```

### Installing Elasticsearch

We need to install Elasticsearch manually:

```
wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.10.1-no-jdk-amd64.deb
sudo dpkg -i --force-all --ignore-depends=libc6 elasticsearch-7.10.1-no-jdk-amd64.deb
```

Open up the status

```
sudoedit /var/lib/dpkg/status 
```

Find the `Package: elasticsearch` section. Change the status from `install ok half-configured` to `install ok installed` and remove `libc6` from `Depends`.  

The next two commands are where the JNA magic happens:

```
sudo mv /usr/share/elasticsearch/lib/jna-5.5.0.jar /usr/share/elasticsearch/lib/jna-5.5.0.jar.old
sudo wget -P /usr/share/elasticsearch/lib https://repo1.maven.org/maven2/net/java/dev/jna/jna/5.5.0/jna-5.5.0.jar
```

> NOTE: Found here: [https://discuss.elastic.co/t/installing-elasticsearch-7-4-on-a-raspberry-pi-4-raspbian-buster/202599/11](https://discuss.elastic.co/t/installing-elasticsearch-7-4-on-a-raspberry-pi-4-raspbian-buster/202599/11)

Avoid keystore permission issues:

```
sudo chmod g+w /etc/elasticsearch
``` 

### Elasticsearch Configuration

Open up the settings:

```
sudoedit /etc/elasticsearch/elasticsearch.yml
```

Add the following to the bottom:

```
xpack.ml.enabled: false
bootstrap.system_call_filter: false
```

Open up more settings:

```
sudoedit /etc/default/elasticsearch
```

And add the following:

```
ES_TMPDIR=/var/log/elasticsearch
```

### Starting the service

Start using `systemctl`:

```
sudo systemctl enable elasticsearch
sudo systemctl start elasticsearch
```

And check to make sure it's wokring:

```
curl http://localhost:9200/_cluster/health?pretty
curl -XGET 'http://localhost:9200'
```

## Setting up SCP with auto credentials

1. Generate an RSA key using PuttyGen. Save the Public and Private keys somewhere.
2. Use PuttyGen to Export OpenSSH key.
3. Copy the text field labeled Public key for pasting into OpenSSH authorized_keys file
4. Paste the contents to `~/.ssh/authorized_keys` on the Raspberry Pi

## Settinh up DynDNS hosting

1. Register here: https://account.dyn.com/
2. Create a new hostname
3. Install the client by running the commands:

```sh
sudo apt update
sudo apt install ddclient
```

4. Configure the client:

Add the following to `/etc/ddclient.conf`:

```
# Configuration file for ddclient generated by debconf
#
# /etc/ddclient.conf

#update every x seconds
daemon=600
# write log to /var/log/syslog
syslog=yes
# use SSL encryption for update requests
ssl=yes

protocol=dyndns2
use=web, web=checkip.dyndns.com, web-skip='IP Address'
server=members.dyndns.org
login=<User Name>
password='<Updater Client Key>'
archives.dyndns.org
```

And add the following to `/etc/default/ddclient`:

```
# Configuration for ddclient scripts
# generated from debconf on Wed 30 Sep 21:54:23 PDT 2020
#
# /etc/default/ddclient

# Set to "true" if ddclient should be run every time DHCP client ('dhclient'
# from package isc-dhcp-client) updates the systems IP address.
run_dhclient="false"

# Set to "true" if ddclient should be run every time a new ppp connection is
# established. This might be useful, if you are using dial-on-demand.
run_ipup="false"

# Set to "true" if ddclient should run in daemon mode
# If this is changed to true, run_ipup and run_dhclient must be set to false.
run_daemon="true"

# Set the time interval between the updates of the dynamic DNS name in seconds.
# This option only takes effect if the ddclient runs in daemon mode.
daemon_interval="300"
```

Then run:

```sh
sudo service ddclient start
sudo service ddclient status
```

Then run `crontab -e` and add the following cron job:

```
0 0 * * 0 /usr/sbin/ddclient -force
```

> NOTE: See https://crontab.guru/every-week for more info on cron job scheduling

## Installing an SSL Certificate

Install certbot, and generate the necessary configuration by running:

```sh
sudo apt update
sudo apt install python3-certbot-nginx
sudo certbot --nginx -d <Your Hostname Here>
```

Then run `crontab -e` and add the following cron job:

```
0 12 * * * /usr/bin/certbot renew --quiet
```

## Migrating a SQL database to SqlLite

Get the sqlite CLI from here: [https://www.sqlite.org/download.html](https://www.sqlite.org/download.html) (or via Chocolatey)

Get the tool from here: [github.com/ErikEJ/SqlCeToolbox](https://github.com/ErikEJ/SqlCeToolbox/wiki/Command-line-tools#command-line-for-export2sqlce)

And run the commands:

```ps
# export the data to a few files
.\Export2SqlCe.exe "Data Source=(local);Initial Catalog=<Your Database Here>;Integrated Security=True" Content.sql sqlite

# create a new sqlite database from the exported data
sqlite3 Database.sqlite ".read Content_0000.sql"
sqlite3 Database.sqlite ".read Content_0001.sql"
sqlite3 Database.sqlite ".read Content_0002.sql"
```

## Checking Logs

Check the logs by running:

```sh
cat /var/log/syslog
cat ~/logs/log<DATE>.txt
```
