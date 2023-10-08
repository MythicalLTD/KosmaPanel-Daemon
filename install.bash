#!/bin/bash
clear 
echo "KosmaPanel Daemon v0.2 Installation Script"
echo "Copyright © 2023 MythicalSystems."
echo "For support join our community: https://discord.gg/7BZTmSK2D8"

sleep 1s
echo ""
if [ "$(id -u)" != "0" ]; then
    printf "This script must be run as root\nYou can login as root with\033[0;32m sudo su -\033[0m\n" 1>&2
    exit 1
fi
read -p "Are you sure you want to continue? [y/n] " installation
if [[ $installation == "y" || $installation == "Y" || $installation == "yes" || $installation == "Yes" ]]
then
    sudo apt update -y && sudo apt upgrade -y
    sudo apt --ignore-missing install nginx git docker containerd docker.io docker-compose -y
    cd /etc
    sudo git clone https://github.com/MythicalLTD/KosmaPanel-Daemon.git KosmaPanel
    sudo mv /etc/KosmaPanel/daemon.service /etc/systemd/system/
    sudo systemctl enable --now daemon.service
    bash arch.bash
    echo "Installing nginx config..."
	clear
    read -p 'Enter your domain ( No IP ): '  domain
    systemctl stop nginx
    certbot certonly --standalone -d $domain
    cp /etc/KosmaPanel/kosmapanel-daemon.conf /etc/nginx/sites-available/kosmapanel-daemon.conf
    sed -i "s/url/$domain/" /etc/nginx/sites-available/kosmapanel-daemon.conf
    ln -s /etc/nginx/sites-available/kosmapanel-daemon.conf /etc/nginx/sites-enabled/kosmapanel-daemon.conf
    rm -r /var/www/html
    mkdir -p /var/www/html
    mkdir -p /var/www/html/errors
    cp /etc/KosmaPanel/templates/index.html /var/www/html
    cp /etc/KosmaPanel/templates/400.html /var/www/html/errors
    cp /etc/KosmaPanel/templates/403.html /var/www/html/errors
    cp /etc/KosmaPanel/templates/404.html /var/www/html/errors
    cp /etc/KosmaPanel/templates/500.html /var/www/html/errors
    cp /etc/KosmaPanel/templates/503.html /var/www/html/errors
    chown -R www-data:www-data /var/www/html/*
    sudo chmod -R 755 /var/www/html/*
    rm /etc/nginx/sites-available/default
    rm /etc/nginx/sites-enabled/default
    cp /etc/KosmaPanel/default.conf /etc/nginx/sites-available/default.conf
    ln -s /etc/nginx/sites-available/default.conf /etc/nginx/sites-enabled/default.conf
    systemctl start nginx
    echo ""
    echo " --> Installation completed"
    echo ""
else
    echo ""
    echo " --> Installation cancelled"
    echo ""
fi