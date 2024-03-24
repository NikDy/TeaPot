# TeaPot
Tepot is Discord bot for playing music in voice channel, for your personal servers only. Therefore, it should not withstand heavy loads by design.


## How to run on ubuntu:
Create a file service, for example, teapot.service, in directory /etc/systemd/system/:

    sudo nano /etc/systemd/system/teapot.service

In the editor that opens, paste the following:

    [Unit]
    Description=TeaPot Service
    After=network.target
    
    [Service]
    ExecStart=path_to_your_executable
    Restart=always
    StandardOutput=syslog
    StandardError=syslog
    SyslogIdentifier=teapot
    
    [Install]
    WantedBy=multi-user.target


>[!IMPORTANT]
>Notice that you need to replace **path_to_your_executable** with the path of your program

After creating the service file, run the command to reload the systemd configuration:

    sudo systemctl daemon-reload

Let's enable the service so that it starts automatically when the system boots:

    sudo systemctl enable teapot.service

You can start the service now, without having to reboot the system:

    sudo systemctl start teapot.service

To verify that the service has successfully started and is running, run:
    
    sudo systemctl status teapot.service


## How to stop
To stop the service you just created, run the following command:

    sudo systemctl stop teapot.service

If you need to temporarily disable autostart of a service at system boot, but leave the service itself in place, you can use the disable command:

    sudo systemctl disable teapot.service

To remove the entire service including the service file, run the following command:

    sudo systemctl disable teapot.service
    sudo rm /etc/systemd/system/teapot.service
    sudo systemctl daemon-reload
