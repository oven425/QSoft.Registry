

# Use Linq style read registry(Linq to Registry)
## Use Queryable read data
* Full Linq support
   provide Queryable function
* Auto control Registry resource
   no control resource create and dispose
## sample code
Create definition
```csharp
public class InstalledApp
{
	public string DisplayName { set; get; }
    public string DisplayVersion { set; get; }
    public int? EstimatedSize { set; get; }
}
```
Create Query
```csharp
var regt = new RegQuery<InstalledApp>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.LocalMachine;
                    x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                });
```
Query data
```csharp
var first1 = regt.First();
var first2 = regt.First(x => x.DisplayName != "");
var last1 = regt.Last();
var last2 = regt.Last(x => x.DisplayName != "");
var take = regt.Take(10);
var takewhile = regt.TakeWhile(x => x.DisplayName == "AA");

var count1 = regt.Count();
var count2 = regt.Count(x => x.DisplayName == "AA");
var all = regt.All(x => x.DisplayName != "");
var any = regt.Any(x => x.EstimatedSize > 0);
var reverse = regt.Reverse();
var average = regt.Average(x => x.EstimatedSize);
var sum = regt.Sum(x => x.EstimatedSize);
var skip1 = regt.Skip(1);
var skipwhile = regt.SkipWhile(x => x.DisplayName == "B");
var min = regt.Min(x => x.EstimatedSize);
var max = regt.Max(x => x.EstimatedSize);

var loopup = regt.ToLookup(x => x.DisplayName);
var tolist = regt.ToList();
var toarray = regt.ToArray();
var dictonary = regt.ToDictionary(x => x.EstimatedSize);
```
```csharp
var orderbydesc = regt.OrderByDescending(x => x.EstimatedSize);
var oderby = regt.OrderBy(x => x.EstimatedSize);
var where1 = regt.Where(x => x.DisplayName != "");
```
Join data definition and create
```csharp
public class AppData
{
    public AppData()
    {

    }

    public AppData(string name)
    {
        this.Name = name;
    }
    public string Name { set; get; }
    public string Ver { set; get; }
    public string Uninstallstring { set; get; }
    public bool IsOfficial { set; get; }
}
```
```csharp
List<AppData> apps = new List<AppData>();
apps.Add(new AppData() { Name = "A", IsOfficial = true });
apps.Add(new AppData() { Name = "AA", IsOfficial = false });
```
```csharp
var join1 = regt.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => new { x.DisplayName, x.EstimatedSize, y.IsOfficial });
var groupjoin = regt.GroupJoin(apps, x => x.DisplayName, y => y.Name, (x, y) => x);
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
