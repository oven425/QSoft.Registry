Welcome to the QSoft.Registry wiki!
# Introduction
*  Support Queryable function
*  Auto control Registry resource no control resource create and dispose
* Support Update and Remove RegistryKey
# Quick Start
```c#
//define want to get data
public class InstalledApp
{
    public string DisplayName { set; get; }
    [RegPropertyName(Name = "DisplayVersion")]
    public Version Version { set; get; }
    public int? EstimatedSize { set; get; }
}

//create registrykey query
var regt = new RegQuery<InstalledApp>()
              .useSetting(x =>
                  {
                      x.Hive = RegistryHive.LocalMachine;
                      x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                      x.View = RegistryView.Registry64;
                  });

//get dispalyname contains Windows
var where = regt.Where(x => x.DisplayName.Contains("Windows"));

//filter version
var where_version = regt.Where(x => x.Version > new Version(1, 1, 1, 1));
```
[More](https://github.com/oven425/QSoft.Registry/wiki)