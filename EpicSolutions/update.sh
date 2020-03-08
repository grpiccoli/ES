sudo tee << EOL /etc/systemd/system/kestrel-epicsolutions.service >/dev/null
[Unit]
Description=EpicSolutions

[Service]
WorkingDirectory=/root/webapps/epicsolutions
ExecStart=/usr/bin/dotnet EpicSolutions.dll
Restart=always
RestartSec=10
SyslogIdentifier=dotnet-epicsolutions
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOL

systemctl daemon-reload

sudo tee << EOL /etc/nginx/sites-available/epicsolutions >/dev/null
server {
    listen 80;
    server_name epicsolutions.cl www.epicsolutions.cl;

    location / {
        proxy_pass              http://localhost:5009;
        proxy_http_version      1.1;
        proxy_set_header        Upgrade \$http_upgrade;
        proxy_set_header        Connection keep-alive;
        proxy_set_header        Host \$host;
        proxy_cache_bypass      \$http_upgrade;
        proxy_set_header        X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header        X-Forwarded-Proto \$scheme;
    }

    if(\$host = epicsolutions.cl) {
        return 301 https://www.\$host\$request_uri;
    }
}
EOL

sudo ln -s /etc/nginx/sites-available/epicsolutions /etc/nginx/sites-enabled/epicsolutions

sudo /usr/sbin/nginx -s reload

git clone https://github.com/grpltda/EpicSolutions.git

dotnet publish -r linux-x64 -c Release
systemctl stop kestrel-epicsolutions.service
rm -r /root/webapps/epicsolutions
mkdir -p /root/webapps/epicsolutions
rsync -auv bin/Release/netcoreapp3.1/linux-x64/publish/* /root/webapps/epicsolutions
systemctl start kestrel-epicsolutions.service