# RemoteConfigDemo

To Use RemoteConfigService

1.Create Configuration Model:
Define a class to represent your configuration data. This class should have properties corresponding to the data you expect to receive from your remote configuration.

```ruby
public class MyConfig
{
    public string Key1 { get; set; }
    public int Key2 { get; set; }
    // Add other properties as needed
}
```

2.Usage:

Create an instance of RemoteConfigService<MyConfig> and call Initialize method with the URL from which you want to fetch your configuration data.

```ruby
RemoteConfigService<MyConfig> configService = new RemoteConfigService<MyConfig>();
string configUrl = "your_config_url";
await configService.Initialize(configUrl);
```

3.Handle Configuration Loaded Event:
Subscribe to the OnConfigLoaded event to handle the scenario when the configuration data is successfully loaded.

```ruby

configService.OnConfigLoaded += OnConfigLoadedHandler;

void OnConfigLoadedHandler()
{
    // Do something with the loaded config data
    Debug.Log("Configuration Loaded!");
}

```

4.Access Configuration Data:
Once the configuration is loaded, you can access the config data through the ConfigData property of RemoteConfigService.

```ruby

string key1Value = configService.ConfigData.Key1;
int key2Value = configService.ConfigData.Key2;

```

Fallback Mechanism:
If fetching configuration fails, the service will attempt to load a fallback configuration from local storage or resources. Make sure you have set up the fallback properly.


# Description of Demo

Overview:
 project that lets users spawn and command two armies of armies in epic battles. Players configure troop count, damage, and special abilities, then watch as their armies clash in real-time combat.

Key Features:

Customizable Armies:
Configure troop count, base damage, and special abilities for each army.

Real-Time Battles:
Witness intense battles between armies factions with calculated outcomes.

Damage Mechanics:
Armies inflict damage based on configured values, with opportunities for double damage.

Upgrade System:
Enhance armies capabilities through upgrades like increased damage or new abilities.

Use GameManager.cs to check the implementation of RemoteConfigService.

#  Note

For several hours each night, I dedicated myself to refining this project. Leveraging an existing army battle demo, I configured the entire project to align with my objectives. Employing a remote config service, I adeptly showcased the module's capabilities, effectively expediting my workflow.