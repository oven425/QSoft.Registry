Welcome to the QSoft.Registry wiki!
# Introduction
*  Support Queryable function
*  Auto control Registry resource no control resource create and dispose
*  Support Update and Remove RegistryKey
*  Support convert value
*  Support subkey query
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

//create convert
public class Version2String : RegQueryConvert<Version>
{
    public override string ConvertTo(Version src)
    {
        return src.ToString();
    }

    public override Version ConvertBack(string dst)
    {
        Version version;
        Version.TryParse(dst, out version);
        return version;
    }
}

//create registrykey query
var regt1 = new RegQuery<InstalledApp>()
      .useSetting(x =>
      {
          x.Hive = RegistryHive.LocalMachine;
          x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
          x.View = RegistryView.Registry64;
      })
      .useConverts(x=>
      {
            x.Add(new Version2String());
      });

//get dispalyname contains Windows
var where = regt.Where(x => x.DisplayName.Contains("Windows"));

//filter version
var where_version = regt.Where(x => x.Version > new Version(1, 1, 1, 1));
```
[More](https://github.com/oven425/QSoft.Registry/wiki)
