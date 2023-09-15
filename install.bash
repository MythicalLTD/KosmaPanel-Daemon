#!/bin/bash
clear 
echo "KosmaPanel Daemon v0.2 Installation Script"
echo "Copyright © 2023 MythicalSystems."
echo "For support join our community: https://discord.gg/7BZTmSK2D8"

sleep 1s
echo ""
# Check if the user is root and if this is an actual user.
if [ "$(id -u)" != "0" ]; then
    printf "This script must be run as root\nYou can login as root with\033[0;32m sudo su -\033[0m\n" 1>&2
    exit 1
fi
read -p "Are you sure you want to continue? [y/n] " installation
if [[ $installation == "y" || $installation == "Y" || $installation == "yes" || $installation == "Yes" ]]
then
    # Update packages
    sudo apt update -y && sudo apt upgrade -y
    # Install Dependency
    sudo apt --ignore-missing install apache2 git docker containerd docker.io docker-compose -y
    # Moving in /etc
    cd /etc
    # Clone stuff
    sudo git clone https://github.com/MythicalLTD/KosmaPanel-Daemon.git KosmaPanel
    sudo mv /etc/KosmaPanel/daemon.service /etc/systemd/system/
    sudo systemctl enable --now daemon.service
    bash arch.bash
    echo "Installing apache2 config..."
	clear
    read -p 'Enter your domain ( No IP ): '  domain
    service apache2 stop
    certbot certonly --standalone -d $domain
    cp /etc/KosmaPanel/daemon.conf /etc/apache2/sites-available/daemon.conf
    sed -i "s/url/$domain/" /etc/apache2/sites-available/daemon.conf
    ln -s /etc/apache2/sites-available/daemon.conf /etc/apache2/sites-enabled/daemon.conf
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
    rm /etc/apache2/sites-available/000-default.conf
    rm /etc/apache2/sites-enabled/000-default.conf
    cp /etc/KosmaPanel/000-default.conf /etc/apache2/sites-available/000-default.conf
    ln -s /etc/apache2/sites-available/000-default.conf /etc/apache2/sites-enabled/000-default.conf
    sudo a2enmod proxy
    sudo a2enmod proxy_http
    sudo a2enmod proxy_wstunnel
    sudo a2enmod rewrite
    sudo a2enmod ssl
    service apache2 start
    echo ""
    echo " --> Installation completed"
    echo ""
else
    echo ""
    echo " --> Installation cancelled"
    echo ""
fi