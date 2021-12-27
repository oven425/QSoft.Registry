#define Queryable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;

namespace ConsoleApp1
{
    public class InstallAppCompare : IEqualityComparer<InstalledApp>
    {
        public bool Equals(InstalledApp x, InstalledApp y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.DisplayName == y.DisplayName;
        }

        public int GetHashCode(InstalledApp obj)
        {
            int hashcode = 0;
            if (object.ReferenceEquals(obj, null))
            {
                hashcode = 0;
            }
            hashcode = obj.DisplayName == null ? 0 : obj.DisplayName.GetHashCode();
            System.Diagnostics.Trace.WriteLine($"hashcode:{hashcode}");
            return hashcode;
        }
    }

    public class User
    {
        public string Key { set; get; }
        public string Name { set; get; }
        public DateTime BirthDay { set; get; }
        public int CityID { set; get; }
    }

    public class City
    {
        public int ID { set; get; }
        public string Name { set; get; }
    }

    public class App
    {
        [RegPropertyName(Name = "DisplayName")]
        public string Name { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
        public string Version { set; get; }
        [RegIgnore]
        public int Size { set; get; }
    }

    public class Company
    {
        public string Name { set; get; }
        public int ID { set; get; }

    }

    public class InstalledApp
    {
        //[RegSubKeyName]
        public string Key { set; get; }
        public string DisplayName { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
        public Version Version { set; get; }
        public int? EstimatedSize { set; get; }
        [RegIgnore]
        public bool? IsOfficial { set; get; }
        public int? ID { set; get; }

        public InstalledApp() { }

        public InstalledApp(InstalledApp data)
        {
            var pps = typeof(InstalledApp).GetProperties().Where(x => x.CanRead == true && x.CanWrite == true);
            foreach (var pp in pps)
            {
                pp.SetValue(this, pp.GetValue(data));
            }
        }
    }

    public class AppMapping
    {
        public int CompanyID { set; get; }
        public int AppID { set; get; }
    }

    public class MyDynamicType
    {
        private int m_number;

        public MyDynamicType() : this(42) { }
        public MyDynamicType(int initNumber)
        {
            m_number = initNumber;
        }

        public int Number
        {
            get { return m_number; }
            set { m_number = value; }
        }

        public int MyMethod(int multiplier)
        {
            return m_number * multiplier;
        }
    }



    class Program
    {
        static void TestTypeBuild()
        {
            AssemblyName aName = new AssemblyName("DynamicAssemblyExample");
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName, AssemblyBuilderAccess.RunAndSave);

            // For a single-module assembly, the module name is usually
            // the assembly name plus an extension.
            ModuleBuilder mb =
                ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            TypeBuilder tb = mb.DefineType(
                "MyDynamicType",
                 TypeAttributes.Public);

            // Add a private field of type int (Int32).
            FieldBuilder fbNumber = tb.DefineField("m_number", typeof(int), FieldAttributes.Private);

            // Define a constructor that takes an integer argument and
            // stores it in the private field.
            Type[] parameterTypes = { typeof(int) };
            ConstructorBuilder ctor1 = tb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes);

            ILGenerator ctor1IL = ctor1.GetILGenerator();
            // For a constructor, argument zero is a reference to the new
            // instance. Push it on the stack before calling the base
            // class constructor. Specify the default constructor of the
            // base class (System.Object) by passing an empty array of
            // types (Type.EmptyTypes) to GetConstructor.
            ctor1IL.Emit(OpCodes.Ldarg_0);
            ctor1IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            // Push the instance on the stack before pushing the argument
            // that is to be assigned to the private field m_number.
            ctor1IL.Emit(OpCodes.Ldarg_0);
            ctor1IL.Emit(OpCodes.Ldarg_1);
            ctor1IL.Emit(OpCodes.Stfld, fbNumber);
            ctor1IL.Emit(OpCodes.Ret);

            // Define a default constructor that supplies a default value
            // for the private field. For parameter types, pass the empty
            // array of types or pass null.
            ConstructorBuilder ctor0 = tb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            ILGenerator ctor0IL = ctor0.GetILGenerator();
            // For a constructor, argument zero is a reference to the new
            // instance. Push it on the stack before pushing the default
            // value on the stack, then call constructor ctor1.
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Ldc_I4_S, 42);
            ctor0IL.Emit(OpCodes.Call, ctor1);
            ctor0IL.Emit(OpCodes.Ret);

            // Define a property named Number that gets and sets the private
            // field.
            //
            // The last argument of DefineProperty is null, because the
            // property has no parameters. (If you don't specify null, you must
            // specify an array of Type objects. For a parameterless property,
            // use the built-in array with no elements: Type.EmptyTypes)
            PropertyBuilder pbNumber = tb.DefineProperty(
                "Number",
                PropertyAttributes.HasDefault,
                typeof(int),
                null);

            // The property "set" and property "get" methods require a special
            // set of attributes.
            MethodAttributes getSetAttr = MethodAttributes.Public |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            // Define the "get" accessor method for Number. The method returns
            // an integer and has no arguments. (Note that null could be
            // used instead of Types.EmptyTypes)
            MethodBuilder mbNumberGetAccessor = tb.DefineMethod(
                "get_Number",
                getSetAttr,
                typeof(int),
                Type.EmptyTypes);

            ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
            // For an instance property, argument zero is the instance. Load the
            // instance, then load the private field and return, leaving the
            // field value on the stack.
            numberGetIL.Emit(OpCodes.Ldarg_0);
            numberGetIL.Emit(OpCodes.Ldfld, fbNumber);
            numberGetIL.Emit(OpCodes.Ret);

            // Define the "set" accessor method for Number, which has no return
            // type and takes one argument of type int (Int32).
            MethodBuilder mbNumberSetAccessor = tb.DefineMethod(
                "set_Number",
                getSetAttr,
                null,
                new Type[] { typeof(int) });

            ILGenerator numberSetIL = mbNumberSetAccessor.GetILGenerator();
            // Load the instance and then the numeric argument, then store the
            // argument in the field.
            numberSetIL.Emit(OpCodes.Ldarg_0);
            numberSetIL.Emit(OpCodes.Ldarg_1);
            numberSetIL.Emit(OpCodes.Stfld, fbNumber);
            numberSetIL.Emit(OpCodes.Ret);

            // Last, map the "get" and "set" accessor methods to the
            // PropertyBuilder. The property is now complete.
            pbNumber.SetGetMethod(mbNumberGetAccessor);
            pbNumber.SetSetMethod(mbNumberSetAccessor);

            // Define a method that accepts an integer argument and returns
            // the product of that integer and the private field m_number. This
            // time, the array of parameter types is created on the fly.
            MethodBuilder meth = tb.DefineMethod(
                "MyMethod",
                MethodAttributes.Public,
                typeof(int),
                new Type[] { typeof(int) });

            ILGenerator methIL = meth.GetILGenerator();
            // To retrieve the private instance field, load the instance it
            // belongs to (argument zero). After loading the field, load the
            // argument one and then multiply. Return from the method with
            // the return value (the product of the two numbers) on the
            // execution stack.
            methIL.Emit(OpCodes.Ldarg_0);
            methIL.Emit(OpCodes.Ldfld, fbNumber);
            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Mul);
            methIL.Emit(OpCodes.Ret);

            // Finish the type.
            Type t = tb.CreateType();

            // The following line saves the single-module assembly. This
            // requires AssemblyBuilderAccess to include Save. You can now
            // type "ildasm MyDynamicAsm.dll" at the command prompt, and
            // examine the assembly. You can also write a program that has
            // a reference to the assembly, and use the MyDynamicType type.
            //
            //ab.Save(aName.Name + ".dll");

            // Because AssemblyBuilderAccess includes Run, the code can be
            // executed immediately. Start by getting reflection objects for
            // the method and the property.
            MethodInfo mi = t.GetMethod("MyMethod");
            PropertyInfo pi = t.GetProperty("Number");

            // Create an instance of MyDynamicType using the default
            // constructor.
            object o1 = Activator.CreateInstance(t);

            // Display the value of the property, then change it to 127 and
            // display it again. Use null to indicate that the property
            // has no index.
            Console.WriteLine("o1.Number: {0}", pi.GetValue(o1, null));
            pi.SetValue(o1, 127, null);
            Console.WriteLine("o1.Number: {0}", pi.GetValue(o1, null));

            // Call MyMethod, passing 22, and display the return value, 22
            // times 127. Arguments must be passed as an array, even when
            // there is only one.
            object[] arguments = { 22 };
            Console.WriteLine("o1.MyMethod(22): {0}",
                mi.Invoke(o1, arguments));

            // Create an instance of MyDynamicType using the constructor
            // that specifies m_Number. The constructor is identified by
            // matching the types in the argument array. In this case,
            // the argument array is created on the fly. Display the
            // property value.
            object o2 = Activator.CreateInstance(t,
                new object[] { 5280 });
            Console.WriteLine("o2.Number: {0}", pi.GetValue(o2, null));
        }
        static void Main(string[] args)
        {
            TestTypeBuild();
            return;

            ////List<int> testlist = Enumerable.Range(1, 5).ToList();
            ////var qur = testlist.AsQueryable();
            ////foreach(var oo in testlist)
            ////{
            ////    if(oo==2)
            ////    {
            ////        testlist[0] = 100;
            ////    }
            ////}




            //TestDB();

            //return;
            //var g1 = zip_method.GetType().GetGenericArguments();
            ////var g2 = zip_method.GetType().GetGenericParameterConstraints();
            //var g3 = zip_method.GetType().GetGenericTypeDefinition();
            List<AppData> apps = new List<AppData>();
            apps.Add(new AppData() { Name = "A", IsOfficial = true });
            apps.Add(new AppData() { Name = "AA", IsOfficial = false });
            //apps.Add(new App() { Name = "B", Offical = true });
            //apps.Add(new App() { Name = "BB", Offical = false });
            //apps.Add(new App() { Name = "C", Offical = true });
            //apps.Add(new App() { Name = "CC", Offical = false });
            //apps.Add(new App() { Name = "D", Offical = true });
            //apps.Add(new App() { Name = "DD", Offical = false });

            List<InstalledApp> installs = new List<InstalledApp>();
            installs.Add(new InstalledApp() { DisplayName = "A", IsOfficial = true });
            installs.Add(new InstalledApp() { DisplayName = "AA", IsOfficial = false });
#if Queryable

            List<int> lll = new List<int>();
            lll.Add(1);
            var aaa = lll.AsQueryable();
            lll.Add(2);

            string full = @"HKEY_LOCAL_MACHINE\";
            //Regex regex1 = new Regex(@"^(?<base>\w+)[\\](\.+)[\\]$(?<path>.+)", RegexOptions.Compiled);
            //Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
            //var match = regex1.Match(full);
            //if(match.Success)
            //{

            //}
            //var sss = full.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            //var ss1 = sss.Skip(1).Take(sss.Length - 2);

            //var ssssss = ss1.Aggregate((x, y) => $"{x}\\{y}");


            //RegistryKey regbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //var reg1 = regbase.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", true);
            //var parent = regbase.GetParent();
            //Regex regex = new Regex(@"[\\]([A-Za-z0-9]+)$");
            //Regex regex = new Regex(@"^(?<base>\w+)[\\]");

            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);


            List<RegistryKey> ll = new List<RegistryKey>();
            foreach (var subkeyname in queryreg.GetSubKeyNames())
            {
                RegistryKey reg = queryreg.OpenSubKey(subkeyname, false);
                ll.Add(reg);
            }
            var queryable = ll.AsQueryable();
            //var o_pp = Expression.Parameter(typeof(RegistryKey), "x");
            //var o1 = typeof(InstalledApp).ToData(o_pp);
            //var o_func = Expression.Lambda<Func<RegistryKey, InstalledApp>>(o1, o_pp).Compile();
            //var o2 = o_func(queryable.First());



            //var rr = queryable.GroupBy(x => x.GetValue<string>("DisplayName"), (x, y) => new { x, y = y.Select(xuu => new InstalledApp() { }) });
            var rr = queryable.Where(x => x.GetValue<DateTime>("CreateTime") == DateTime.Now);
            var ttype = rr.GetType();
            MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            var unary = methodcall.Arguments[1] as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;
            var binary = lambda.Body as BinaryExpression;
            var property = binary.Right as MemberExpression;
            ttype = binary.Right.GetType();



            //var newexpr = lambda.Body as NewExpression;
            //methodcall = newexpr.Arguments[1] as MethodCallExpression;
            //var ggw = methodcall.Method.GetGenericArguments();
            //lambda = methodcall.Arguments[1] as LambdaExpression;
            //ttype = methodcall.Arguments[1].GetType();
            //Microsoft.Win32.Registry.CurrentConfig.CreateSubKey("TT", true);
            //RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64).CreateSubKey("TT", true);
            //var direct_reg = new RegQuery<A2>()
            //   .useSetting(x =>
            //   {
            //       x.Hive = RegistryHive.CurrentConfig;
            //       x.SubKey = @"Test";
            //       x.View = RegistryView.Registry64;
            //   });
            //direct_reg.Create(new A2() { A = "12345" }, true);
            //var a2 = direct_reg.Get();
            //a2.A = "098876";
            //direct_reg.Update(a2);
            //direct_reg.Delete();

            var regt_city = new RegQuery<City>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"Citys";
                    x.View = RegistryView.Registry64;
                });
            //regt_city.RemoveAll();
            //regt_city.Insert(new List<City>()
            //{
            //    new City(){ID=0, Name="Kaohsiung"},
            //    new City(){ID=1, Name="New Taipei"},
            //    new City(){ID=2, Name="Taichung"},
            //    new City(){ID=3, Name="Tainan"},
            //});

            var regt_user = new RegQuery<User>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"Users";
                    x.View = RegistryView.Registry64;
                });
            //regt_user.RemoveAll();
            //regt_user.Insert(new List<User>()
            //{
            //    new User(){Name="AAA", BirthDay=new DateTime(1980, 1,1), CityID=0},
            //    new User(){Name="AAA_1", BirthDay=new DateTime(1980, 1,1), CityID=0},
            //    new User(){Name="BBB", BirthDay=new DateTime(1981, 1,1), CityID=1},
            //    new User(){Name="CCC", BirthDay=new DateTime(1982, 1,1), CityID=1},
            //    new User(){Name="DDD", BirthDay=new DateTime(1983, 1,1), CityID=2},
            //    new User(){Name="EEE", BirthDay=new DateTime(1984, 1,1), CityID=2},
            //    new User(){Name="FFF", BirthDay=new DateTime(1985, 1,1), CityID=3}
            //});
            //var ddd = regt_user.FirstOrDefault().BirthDay;
            //var oi = regt_user.Where(x => x.BirthDay == regt_user.FirstOrDefault().BirthDay).RemoveAll();
            //foreach (var oo in oi)
            //{

            //}

            //var jj = regt_user.Join(regt_city, x => x.CityID, x => x.ID, (x, y) => new { Name = x.Name, City = y.Name }).OrderBy(x => x.Name);
            //jj.ToList();
            var groupby_age = regt_user.GroupBy(x => x).Select(x=>x.Key);
            foreach(var oo in groupby_age)
            {

            }
            var regt = new RegQuery<InstalledApp>()
                .useSetting(x =>
                    {
                        x.Hive = RegistryHive.LocalMachine;
                        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
                        x.View = RegistryView.Registry64;
                    });

            var regt1 = new RegQuery<InstalledApp>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.LocalMachine;
                    x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
                    x.View = RegistryView.Registry64;
                });
            //regt.Insert(Enumerable.Repeat(new InstalledApp(), 3));
            //int remove_count2 = regt.RemoveAll();
            //var yj = regt1.Take(2);
            //var aaaa = regt.Except(regt1.Take(2));
            //var aaaa = regt.Where(x=>x.DisplayName!="").Except(regt1.Take(2));
            //var aaaa = regt.Except(installs, new InstallAppCompare());
            //var aaaa = regt.Union(regt.ToList().Take(2));
            //var aaaa = regt.Select(x => x).Where(x => x.DisplayName != "");
            //var aaaa = regt.Where(x => x.Key == $"{x.Key}");
            //foreach (var oo in aaaa)
            //{

            //}
            //int update_count = regt.Update(x => new InstalledApp()
            //{
            //    EstimatedSize = x.EstimatedSize + 100,
            //    ModifyTime = new DateTime(2222, 2, 2)
            //});
            //var tt = regt.First().CreateTime;


            //var fir = regt.First();
            ////var ttk = regt.TakeWhile((x, index) => index == 0);
            ////int revv = regt.RemoveAll();
            //var tuple = regt.Select(x => Tuple.Create(x.DisplayName, x.EstimatedSize));
            //var tuple1 = regt.Select((x,idx) => Tuple.Create(x.DisplayName, idx));
            //var ssssz = regt.Where(x=>x.DisplayName=="AA").RemoveAll();
            //var group1 = regt.Where(x=>x.DisplayName=="").GroupBy(x => x.DisplayName, (key, reg) => reg);
            var avg = regt.Where(x => int.Parse(x.EstimatedSize.ToString()).ToString("X2") == "0A").Select(x=>x.DisplayName);
            foreach(var oo in avg)
            {

            }
            //var group4 = regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => data.Count());
            List<int> src1 = new List<int> { 1, 2, 3, 7, 7 };
            List<int> src2 = new List<int>() { 3, 4, 5 };
            //var dst1 = src1.Except(src2);
            //var dst2 = src1.Intersect(src2);
            //var dst3 = src1.Union(src2);
            //var dst4 = src1.Distinct();

            var dst1 = src1.Except(src1);
            var dst2 = src1.Intersect(src1);
            var dst3 = src1.Union(src1);


            //List<InstalledApp> src1 = new List<InstalledApp>
            //{
            //    new InstalledApp() { DisplayName = "1" },
            //    new InstalledApp() { DisplayName = "2" },
            //    new InstalledApp() { DisplayName = "3" },
            //    new InstalledApp() { DisplayName = "7" },
            //    new InstalledApp() { DisplayName = "7" },
            //};
            //List<InstalledApp> src2 = new List<InstalledApp>
            //{
            //    new InstalledApp() { DisplayName = "3" },
            //    new InstalledApp() { DisplayName = "4" },
            //    new InstalledApp() { DisplayName = "5" },
            //};
            //var dst1 = src1.Except(src2, new InstallAppCompare());
            //var dst2 = src1.Intersect(src2, new InstallAppCompare());
            //var dst3 = src1.Union(src2, new InstallAppCompare());
            //var dst4 = src1.Distinct(new InstallAppCompare());


            //var group1 = regt.GroupBy(x => x.DisplayName); //1146
            //var group2 = regt.GroupBy(x => x.DisplayName, y => y.DisplayName);//1074
            //var group3 = regt.GroupBy(x => x.DisplayName, (key, reg) => key);//1043
            //var group4 = regt.GroupBy(x => x.DisplayName, (key, reg) => reg);//1043
            //var group5 = regt.GroupBy(x => x.DisplayName, (key, reg) => new { reg });//1043
            //foreach(var item in group5)
            //{
            //    foreach(var oo in item.reg)
            //    {

            //    }
            //}



            var group7 = regt.GroupBy(x => x.DisplayName, y=>y.EstimatedSize, (x,e)=>new { x,e});//1006

            //var fi = regt.FirstOrDefault();
            //try
            //{
            //    int update_count2 = regt.Where(x=>x.IsOfficial==true).Update(x => new { EstimatedSize = x.EstimatedSize + 100 });
            //}
            //catch(Exception ee)
            //{

            //}
            var og = regt.First();
            //int update_count12 = regt.Where(x => x.EstimatedSize > 130).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            //var group2 = regt.GroupBy(x => x.DisplayName, (key, app) => new { key, app });
            var takewhile1 = regt.AsEnumerable();
            var a = (a:123, b:"123");
            int.TryParse("100", out var bbbb);
            //foreach (var oo in takewhile1)
            //{

            //}
            var aa = regt.GroupBy(x => x.Version, (ver, app)=>new { ver, app});
            regt.GroupBy(x=>x, (x,y)=>x).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            //regt.Where(x => x.EstimatedSize > 130).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            //var where_select = regt.Where(x => x.EstimatedSize > 130).Select((x, index) => x);
            //var group1 = regt.GroupBy(x => x.DisplayName);
            //var group2 = regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize);
            //var group3 = regt.GroupBy(x => x.DisplayName, (y, z) => z);
            //foreach (var oo in group3)
            //{

            //}
            //var where = regt.Where(x => x.DisplayName != ""&& x.EstimatedSize>0);
            var zip = regt.Zip(installs, (reg, app) => reg);
            var select = regt.Select((x, index) => index+1);
            var any = regt.Any(x => x.EstimatedSize > 0);
            var first1 = regt.First();
            var first2 = regt.First(x => x.DisplayName != "");
            var last1 = regt.Last();
            var last2 = regt.Last(x => x.DisplayName != "");
            var take = regt.Take(10);
            var takewhile = regt.TakeWhile(x => x.DisplayName == "AA");

            var count1 = regt.Count();
            var count2 = regt.Count(x => x.DisplayName == "AA");
            var all = regt.All(x => x.DisplayName != "");
            
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
            //var max = regt.Max(x => x.DisplayVersion.ToString().Length);
            //var loopup = regt.ToLookup(x => x.DisplayName);
            //var tolist = regt.ToList();
            //var toarray = regt.ToArray();
            //var dictonary = regt.ToDictionary(x => x.EstimatedSize);
            //var single = regt.Single(x => x.DisplayName == "A");
            //var singledefault = regt.SingleOrDefault(x => x.DisplayName == "A");

#else

            //電腦\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}
            var ll = RegistryHive.LocalMachine.OpenView64(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}", true);
            var uus = ll.Take(x=>x!= "Properties", writable: true).Where(y=>y.GetValue<string>("AdapterModel") == "Intel(R) Dual Band Wireless-AC 7265");
            foreach(var uu in uus)
            {
                uu.SetValue("Is6GhzBandSupported", 1);
            }
            System.Diagnostics.Trace.WriteLine(ll.GetValue<string>("AdapterModel"));
            List<CTabble1> table1s = new List<CTabble1>();
            table1s.Add(new CTabble1() { Key1 = "1", Name1 = "table1_1" });
            table1s.Add(new CTabble1() { Key1 = "2", Name1 = "table1_2" });
            table1s.Add(new CTabble1() { Key1 = "3", Name1 = "table1_3" });

            List<CTable2> table2s = new List<CTable2>();
            table2s.Add(new CTable2() { Key2 = "1", Name2 = "table2_1" });
            table2s.Add(new CTable2() { Key2 = "1", Name2 = "table2_1" });
            table2s.Add(new CTable2() { Key2 = "2", Name2 = "table2_2" });
            table2s.Add(new CTable2() { Key2 = "3", Name2 = "table2_3" });

            var table = table1s.Join(table2s, x => x.Key1, y => y.Key2, (x, y) => new { x, y });
            var regs = RegistryHive.LocalMachine.OpenView(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach(var reg in regs)
            {

            }
            //RegistryKey reg_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            //RegistryKey reg_64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey reg_32 = RegistryHive.LocalMachine.OpenView32(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            RegistryKey reg_64 = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");

            //RegistryKey win_info = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            //string ReleaseId =  win_info.GetValue<string>("ReleaseId");
            RegistryKey win_info = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\Microsoft\");
            string ReleaseId = win_info.GetValue<string>(@"Windows NT\CurrentVersion","ReleaseId");
            //SOFTWARE\Microsoft\Windows NT\CurrentVersion
            RegistryKey uninstall = reg_64.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            //foreach (var oo in uninstall.GetSubKeyNames())
            //{
            //    RegistryKey subkey = uninstall.OpenSubKey(oo);
            //    object obj = subkey.GetValue("DisplayName");
            //    string displayname = subkey.GetValue("DisplayName") as string;
            //    System.Diagnostics.Trace.WriteLine(displayname);
            //    subkey.Dispose();
            //}

            List<AppData> apps = new List<AppData>();
            apps.Join(apps, x => x.Name, y => y.Name, (x,y)=> new {x, y }, StringComparer.OrdinalIgnoreCase);
            apps.Add(new AppData() { Name = "WinFlash" });
            apps.Add(new AppData() { Name = "WinFlash" });
            apps.Add(new AppData() { Name = "Dropbox 25 GB" });
            apps.Add(new AppData() { Name = "AnyDes", IsOfficial = true });
            //var apptest = apps.Join(apps, func=> { return true; }, (x, y) => new { x, y });
            //apptest.ToList();
            foreach (var app in apps)
            {
                var reg1 = uninstall.FirstOrDefault(x => x.GetValue<string>("DisplayName") == app.Name);
                if (reg1 != null)
                {
                    //app.Ver = reg1.GetValue<string>("DisplayVersion");
                    //app.Uninstallstring = reg1.GetValue<string>("UninstallString");
                }
            }
            Func<AppData, RegistryKey, AppData> f = ((a, b) =>
            {
                a.Name = b.GetValue<string>("DisplayName");
                a.Uninstallstring = b.GetValue<string>("UninstallString");
                a.Ver = b.GetValue<string>("DisplayVersion");
                return a;
            });
            //var jj = uninstall.Where(x => x.GetValue<string>("DisplayName") != "").Join(apps, x => x.GetValue<string>("DisplayName"), y => y.Name, (x, y) => new { x, y }).Select(x => f(x.y, x.x));
            //var jj = uninstall.Join(apps, x => x.GetValue<string>("DisplayName"), y => y.Name, (x, y) => new { x, y });
            var jj = uninstall.Join(apps, reg => reg.GetValue<string>("DisplayName"), app => app.Name, (reg, app) => new { reg, app })
                .Select(so =>
                {
                    so.app.Uninstallstring = so.reg.GetValue<string>("UninstallString");
                    so.app.Ver = so.reg.GetValue<string>("DisplayVersion");
                    return so.app;
                });
            var existapps = uninstall.Join(apps, (reg, app) =>
            {
                bool hr = false;
                string dispay = reg.GetValue<string>("DisplayName");
                string uninstall_ = reg.GetValue<string>("Uninstall");
                string version = reg.GetValue<string>("Version");
                if (app.IsOfficial == true)
                {
                    hr = string.IsNullOrEmpty(dispay) == false && app.Name.Contains(dispay);
                }
                else
                {
                    hr = app.Name == dispay;
                }

                return hr;
            }, (reg, app) => new { reg, app })
                .Select(so =>
                {
                    so.app.Uninstallstring = so.reg.GetValue<string>("UninstallString");
                    so.app.Ver = so.reg.GetValue<string>("DisplayVersion");
                    return so.app;
                });
            int runcount = 50;
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            for(int i=0; i< runcount; i++)
            {
                //foreach (var oo in uninstall.GetSubKeyNames())
                //{
                //    RegistryKey subkey = uninstall.OpenSubKey(oo);
                //    object obj = subkey.GetValue("DisplayName");
                //    //object obj1 = subkey.GetValue("UninstallString");
                //    //object obj2 = subkey.GetValue("DisplayVersion");
                //    //string displayname = subkey.GetValue("DisplayName") as string;
                //    //System.Diagnostics.Trace.WriteLine(displayname);
                //    subkey.Dispose();
                //}


                existapps.ToList();
            }
            sw.Stop();
            System.Diagnostics.Trace.WriteLine($"{sw.ElapsedMilliseconds/ runcount}");
            //foreach (var oo in existapps)
            //{

            //}

            //var nonexist = apps.Except(existapps);
            var jjj = uninstall.Select(x => new { DisplayName = x.GetValue<string>("DisplayName"), DisplayVersion = x.GetValue<string>("DisplayVersion") });
            foreach (var oo in jjj)
            {
                //oo.DisplayName;
            }


            var first = uninstall.FirstOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics" || x.GetValue<string>("DisplayName") == "");
            var last = uninstall.LastOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
            var count = uninstall.Count();
            var count_1 = uninstall.Count(x => string.IsNullOrEmpty(x.GetValue<string>("DisplayName")) == false);
            var select = uninstall.Select(x => x.GetValue<string>("DisplayName"));
            var where = uninstall.Where(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
            var list = uninstall.ToList();
            var dic = uninstall.ToDictionary(x => x.Name);
            var groups = uninstall.GroupBy(x => x.GetValue<string>("DisplayName"));
            foreach (var item in groups)
            {
                System.Diagnostics.Trace.WriteLine($"DisplayName:{item.Key} count:{item.Count()}");
                foreach (var oo in item)
                {
                    //System.Diagnostics.Trace.WriteLine($"DisplayName:{oo.GetValue<string>("DisplayName")}");
                }
            }
            var lookups = uninstall.ToLookup(x => x.GetValue<string>("DisplayName"));
            foreach (var item in lookups)
            {
                System.Diagnostics.Trace.WriteLine($"DisplayName:{item.Key} count:{item.Count()}");
                foreach (var oo in item)
                {
                    //System.Diagnostics.Trace.WriteLine($"DisplayName:{oo.GetValue<string>("DisplayName")}");
                }
            }
            var takes = uninstall.Take(3);

#endif

        }

        public static void TestDB()
        {
            RegQuery<AppMapping> regt_appmapping = new RegQuery<AppMapping>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\AppMapping";
                x.View = RegistryView.Registry64;
            });

            RegQuery<Company> regt_company = new RegQuery<Company>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"UnitTest\Company";
                    x.View = RegistryView.Registry64;
                });


            RegQuery<InstalledApp> regt_apps = new RegQuery<InstalledApp>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"UnitTest\Apps";
                });
            //regt_company.RemoveAll();
            //regt_company.Insert(new List<Company>()
            //{
            //    new Company(){Name = "Company_A", ID=1},
            //    new Company(){Name = "Company_B", ID=2},
            //    new Company(){Name = "Company_C", ID=3},
            //    new Company(){Name = "Company_D", ID=4},
            //    new Company(){Name = "Company_E", ID=5},
            //});
            regt_apps.RemoveAll();
            regt_apps.Insert(new List<InstalledApp>()
            {
                new InstalledApp() { Key="AA", DisplayName = "AA", Version = new Version("1.1.1.1"), EstimatedSize = 10, ID=0},
                new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.2"), EstimatedSize = 10, ID = 1 },
                new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.3"), EstimatedSize = 10, ID = 2 },
                new InstalledApp() { Key = "BB", DisplayName = "BB", Version = new Version("2.2.2.2"), EstimatedSize = 20, ID = 3 },
                new InstalledApp() { Key = "CC", DisplayName = "CC", Version = new Version("3.3.3.3"), EstimatedSize = 30, ID = 4 },
                new InstalledApp() { Key = "DD", DisplayName = "DD", Version = new Version("4.4.4.4"), EstimatedSize = 40, ID = 5 },
                new InstalledApp() { Key = "EE", DisplayName = "EE", Version = new Version("5.5.5.5"), EstimatedSize = 50, ID = 6 },
                new InstalledApp() { Key = "FF", DisplayName = "FF", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 7 }
            });

            regt_appmapping.RemoveAll();
            regt_appmapping.Insert(new List<AppMapping>()
            {
                new AppMapping(){AppID = 0, CompanyID = 1},
                new AppMapping(){AppID = 1, CompanyID = 1},
                new AppMapping(){AppID = 2, CompanyID = 1},
                new AppMapping(){AppID = 3, CompanyID = 1},
                new AppMapping(){AppID = 4, CompanyID = 2}
            });
            //var applist = from company in regt_company where company.ID==1 select new { company };
            //var applist = from company in regt_company
            //              join appmapping in regt_appmapping
            //              on company.ID equals appmapping.CompanyID into gj
            //              from subpet in gj.DefaultIfEmpty()
            //              select new { company.ID, company.Name, appid=subpet.AppID};
            //var gg = regt_company.Join(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new { x, y })
            //    .Join(regt_apps, x => x.y.AppID, y => y.ID, (x, y) => new { x, y }).GroupBy(x=>x.x.x.Name);

            //var mapping = regt_appmapping.ToList();
            //regt_appmapping.Select((x, index) => new { x, index});
            //regt_appmapping.Select(x => new { x }).ToList();
            //regt_appmapping.Join(regt_apps, x => x.AppID, y => y.ID, (x, y) => new { x, y }).ToList();

            //var join = regt_company.Join(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new {x,y });
            //var gj = regt_company.GroupJoin(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new { x, y }).ToList();


            var left2 = regt_company.GroupJoin(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new { x, y })
                .SelectMany(a => a.y.DefaultIfEmpty(), (x, y) => new { x.x, y });

            var left1 = from company in regt_company
                       join map in regt_appmapping on company.ID equals map.CompanyID into temp
                       from mapresult in temp.DefaultIfEmpty()
                       select new { company, mapresult };
            //left1.Where(x => x.mapresult == null).Select(x => x.company).RemoveAll();


            //var left11 = (from map in regt_appmapping
            //              join comapny in regt_company on map.CompanyID equals comapny.ID into companys
            //              from companyhr in companys.DefaultIfEmpty()
            //              join app in regt_apps on map.AppID equals app.ID into apps
            //              from apphr in apps.DefaultIfEmpty()
            //              select new { companyhr.Name, apphr.DisplayName });

            //var inner = from company in regt_company
            //            from appmapping in regt_appmapping
            //            from app in regt_apps
            //            where company.ID == appmapping.CompanyID && appmapping.AppID == app.ID
            //            select new { company.Name };
        }
    }

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

    public class A2
    {
        public string A { set; get; }
    }

    public class BIOS
    {
        [RegSubKeyName]
        public string Key { set; get; }
        public string BaseBoardManufacturer { set; get; }
        public string BaseBoardProduct { set; get; }
        public string BaseBoardVersion { set; get; }
        public uint BiosMajorRelease { set; get; }
        public int BiosMinorRelease { set; get; }
        public string BIOSReleaseDate { set; get; }
        public string BIOSVendor { set; get; }
        public string BIOSVersion { set; get; }
        public int ECFirmwareMajorRelease { set; get; }
        public int ECFirmwareMinorRelease { set; get; }
        public string SystemFamily { set; get; }
        public string SystemManufacturer { set; get; }
        public string SystemProductName { set; get; }
        public string SystemSKU { set; get; }
        public string SystemVersion { set; get; }
    }
}
