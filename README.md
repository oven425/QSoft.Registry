# Use Linq style read registry(Linq to Registry)
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
