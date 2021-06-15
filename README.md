# DcsMissionValidator
This tool will check a DCS mission.miz, if it contains invalid mods (and other things).

The tool is a command line tool. Start it with parameter -? to read about different options.




Samples:

DcsMissionValidator.exe -dir:"D:\MyMissions" -txt

DcsMissionValidator.exe -file:"D:\MyMissions\SuperMission.miz"

DcsMissionValidator.exe -file:"D:\MyMissions\SuperMission.miz" -dir:"D:\MyMissions" -txt -sim

DcsMissionValidator.exe -rec:"D:\MyMissions" 

DcsMissionValidator.exe -mon:"D:\MyMissions" -txt

DcsMissionValidator.exe -file:"D:\MyMissions\SuperMission.miz" -cfg:"d:\myconfig.xml"
