# FourOneShot.Pi.Blinds
A Blazor web app and .NET web API for Raspberry Pi, for controlling window roller blinds.
## Hardware
The project requires a Raspberry Pi with network connectivity and GPIO pins, a relay module with at least 6 relays and a sacrificial remote control for the blinds.
The GPIO pins are used to control the relays and the relay outputs (NC and common) are wired directly across the remote control switches to emulate button presses.

The remote control used in this project is a Doya DD2702H (re-branded for the local retailer).
Any similar remote can work if it has the same button functions and sleep behaviour, or you can copy the code from DD2702H.cs to a new class and modify to suit your remote.

Relays are optional, if you manage to drive the remote control MCU input pins directly from the Pi's GPIO (not so simple if the remote uses a pull-up configuration).
Or you can use FETs, opto-couplers, etc. However, relays are a simple brute-force method if you don't want to spend too much time reverse engineering the remote circuit.

Aside from powering the Raspberry Pi, you might need a higher voltage (9-12V) supply for your relay module. USB-C PD "trigger" boards with voltage selection can be useful.
You may be able to power the remote control directly from the 3.3V rail of the Raspberry Pi, or you can keep it isolated by using a battery source.

![image](https://github.com/user-attachments/assets/7b70b774-7998-4d86-bd50-621639355f7d)

## Environment set-up
These are the rough steps to follow, but expect some glaring omissions or innacuracies.
It requires some working knowledge of .NET development and Debian Linux (or alternatively, liberal use of search engines and AI helper tools).

Pre-requisite: All the hardware should already be set-up at this point and the remote control should be paired with the blinds you want to control.

1.) Set-up the Raspberry Pi with Pi OS and connect it to your local network.

2.) Install ufw on the Pi (optional, but recommended if you want to lock down network access).

3.) Enable SSH on the Pi (optional, but recommended for managing the Pi remotely).

3.) Set-up a Samba shared network folder on the Pi (optional, but very useful for publishing the web app and API).

4.) Ensure you have Visual Studio (with web dev workload) or Rider with .NET 8 SDK installed on your dev machine.

5.) Open the solution and ensure it builds without errors.

6.) Open appsettings.json in the API project and edit the channel mappings to match your remote control and blinds.

7.) Publish the web app and API projects for linux-arm64, using the self-contained option.

8.) Copy the published folders to the Pi (unless you've published directly to a shared folder already).

9.) Ensure that FourOneShot.Pi.API and FourOneShot.Pi.Blinds have execute permissions.

10.) Configure ufw (if installed) to allow TCP traffic from the local network to port 5001.

11.) Optionally allow TCP traffic to port 5000 also, if you want to expose the API.

12.) Add service configurations for the web app and API, so that they start automatically after booting Pi OS.

13.) Re-boot the Pi and use the systemctl status command command to ensure both services are running.

14.) Try to browse to http://localhost:5001/ on the Pi desktop and cross your fingers.

15.) If that works (or you're running Pi OS headless), browse to that port on the Pi's address from another host.

![image](https://github.com/user-attachments/assets/95b3bf93-6a17-4606-a95b-ab95248a3020)

## Automation

You can use the Linux cron tab to schedule blind opening and closing times.
Create shell scripts that use curl to call the API and then add entries to the cron tab to execute the scripts at specific times.
Use sleep commands and calls to api/blinds/stop to perform partial opening/closing.
Start the API project in Visual Studio to explore the API in Swagger UI (run with IIS Express to open in the browser automatically).

