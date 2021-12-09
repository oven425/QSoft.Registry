# Use Linq style read registry(Linq to Registry)
## Use Queryable read data
* Support Queryable function
* Auto control Registry resource no control resource create and dispose
* Support Update RegistryKey
## Sample code
Create definition
```csharp
public class InstalledApp
{
	public string DisplayName { set; get; }
    public string DisplayVersion { set; get; }
    public int? EstimatedSize { set; get; }
}
```
if you want to use exist class, can add attribute,like below code
```csharp
public class App
{
    [RegPropertyName(Name = "DisplayName")]
    public string Name { set; get; }
    [RegPropertyName(Name = "DisplayVersion")]
    public string Version { set; get; }
    [RegIgnore]
    public int Size { set; get; }
}
```


Create Query
```csharp
var regt = new RegQuery<InstalledApp>()
	.useSetting(x =>
	{
		x.Hive = RegistryHive.LocalMachine;
		x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
		x.View = RegistryView.Registry64;
	});
```
Get data
```csharp
var first1 = regt.First();
var first2 = regt.First(x => x.DisplayName != "");
var last1 = regt.Last();
var last2 = regt.Last(x => x.DisplayName != "");


var count1 = regt.Count();
var count2 = regt.Count(x => x.DisplayName == "AA");
var all = regt.All(x => x.DisplayName != "");
var any = regt.Any(x => x.EstimatedSize > 0);
var reverse = regt.Reverse();
var average = regt.Average(x => x.EstimatedSize);
var sum = regt.Sum(x => x.EstimatedSize);
var min = regt.Min(x => x.EstimatedSize);
var max = regt.Max(x => x.EstimatedSize);

var loopup = regt.ToLookup(x => x.DisplayName);
var tolist = regt.ToList();
var toarray = regt.ToArray();
var dictonary = regt.ToDictionary(x => x.EstimatedSize);
```
Query data
```csharp
var take = regt.Take(10);
var takewhile = regt.TakeWhile(x => x.DisplayVersion <= new Version(2,2,2,2));
var orderbydesc = regt.OrderByDescending(x => x.DisplayVersion);
var oderby = regt.OrderBy(x => x.EstimatedSize);
var where = regt.Where(x => x.DisplayName != "" && x.DisplayVersion > new Version(3,3,3,3));
```
Update Data
```csharp
//update all data
int update_count1 = regt.Update(x => new InstalledApp() { DisplayName = $"{x.DisplayName}_AA" });
int update_count2 = regt.Update(x => new { DisplayName = $"{x.DisplayName}_AA" });
//update by rule
int update_count3 = regt.Where(x=>x.Version>new Version(1,1,1,1)).Update(x => new InstalledApp() { DisplayName = $"{x.DisplayName}_AA" });
int update_count4 = regt.Where(x=>x.Version>new Version(1,1,1,1)).Update(x => new { DisplayName = $"{x.DisplayName}_AA" });
```
GroupBy Data
```csharp
var group1 = regt.GroupBy(x => x.DisplayName);
var group2 = regt.GroupBy(x => x.DisplayName, (key, app) => new { key, app });
```




## Use extension linq function like below code
```csharp
using QSoft.Registry;
using QSoft.Registry.Linq;

RegistryKey reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
RegistryKey uninstall = reg.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
var where = uninstall.Where(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
```
### Open RegistryKey
```csharp
var regs = RegistryHive.LocalMachine.OpenView(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
foreach(var reg in regs)
{

}
RegistryKey reg_32 = RegistryHive.LocalMachine.OpenView32(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
RegistryKey reg_64 = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
```
## Current provide
### FirstOrDefault, LastOrDefault
```csharp
var first = uninstall.FirstOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
var last = uninstall.LastOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
```
### Count
```csharp
var count = uninstall.Count();
var count_1 = uninstall.Count(x => string.IsNullOrEmpty(x.GetValue<string>("DisplayName")) == false);
```
### Where
```csharp
var where = uninstall.Where(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
```
### ToList, ToDictionary
```csharp
var list = uninstall.ToList();
var dic = uninstall.ToDictionary(x => x.Name);
```
### ToLookup
```csharp
var lookups = uninstall.ToLookup(x => x.GetValue<string>("DisplayName"));
foreach (var item in lookups)
{
    System.Diagnostics.Trace.WriteLine($"DisplayName:{item.Key} count:{item.Count()}");
}
```
