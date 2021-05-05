# publish uysing dotnet
dotnet publish "./src/TheArchives/TheArchives.csproj" -o "./publish" -c "Release"

# copy files using SCP
scp -r -i "C:\my keys\raspberrrypi-openssh" "./publish" "pi@192.168.0.11:~/publish_swap"
scp -r -i "C:\my keys\raspberrrypi-openssh" "./src/TheArchives/Database.sqlite" "pi@192.168.0.11:~/publish_swap"

# swap with old files
ssh -i "C:\my keys\raspberrrypi-openssh" "pi@192.168.0.11" "'mv' /home/pi/publish /home/pi/publish_old && 'mv' /home/pi/publish_swap /home/pi/publish"

# restart Kestral
ssh -i "C:\my keys\raspberrrypi-openssh" "pi@192.168.0.11" "sudo systemctl restart kestrel-empowerapp.service"

# remove swap dir
ssh -i "C:\my keys\raspberrrypi-openssh" "pi@192.168.0.11" "'rm' -rf /home/pi/publish_old"