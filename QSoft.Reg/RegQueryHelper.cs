using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QSoft.Registry.Linq
{
    public static class RegQueryHelper
    {
        static int m_BuildTypeCount = 0;
        static void AddProperty(this TypeBuilder tb, FieldBuilder fbNumber, bool canread, bool canwrite)
        {
            if(canread == false && canwrite==false)
            {
                return;
            }
            string name = fbNumber.Name.Remove(0,2);
            PropertyBuilder pbNumber = tb.DefineProperty(name, PropertyAttributes.HasDefault, fbNumber.FieldType, null);
            
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            if(canread == true)
            {
                MethodBuilder mbNumberGetAccessor = tb.DefineMethod($"get_{name}", getSetAttr, fbNumber.FieldType, Type.EmptyTypes);
                ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();

                numberGetIL.Emit(OpCodes.Ldarg_0);
                numberGetIL.Emit(OpCodes.Ldfld, fbNumber);
                numberGetIL.Emit(OpCodes.Ret);
                pbNumber.SetGetMethod(mbNumberGetAccessor);
            }

            if (canwrite == true)
            {
                MethodBuilder mbNumberSetAccessor = tb.DefineMethod($"set_{name}", getSetAttr, null, new Type[] { fbNumber.FieldType });
                ILGenerator numberSetIL = mbNumberSetAccessor.GetILGenerator();
                numberSetIL.Emit(OpCodes.Ldarg_0);
                numberSetIL.Emit(OpCodes.Ldarg_1);
                numberSetIL.Emit(OpCodes.Stfld, fbNumber);
                numberSetIL.Emit(OpCodes.Ret);

                pbNumber.SetSetMethod(mbNumberSetAccessor);
            }
        }

        public static Type BuildType(this IEnumerable<Tuple<Type, string>> types, IEnumerable<Type> exists)
        {
            
            AssemblyName aName = new AssemblyName("RegQuery");
            AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name, aName.Name + ".dll");

            Interlocked.Increment(ref m_BuildTypeCount);
            TypeBuilder tb = mb.DefineType($"q_AnyoumusType_{types.Count()}_{exists?.Count()}_{m_BuildTypeCount}", TypeAttributes.Public);
            
            List<KeyValuePair<string, Type>> typekeys = new List<KeyValuePair<string, Type>>();

            var tts = new List<Tuple<Type, string>>();
            var types1 = types.ToList();
            var exists_count = 0;
            if(exists != null)
            {
                exists_count = exists.Count();
            }
            int existindex = types1.Count - exists_count;
            for (int i=0; i<types1.Count; i++)
            {
                if(i< existindex)
                {
                    tts.Add(Tuple.Create(types1[i].Item1, types1[i].Item2));
                    typekeys.Add(new KeyValuePair<string, Type>(types1[i].Item2, types1[i].Item1));
                }
                else
                {
                    tts.Add(Tuple.Create(exists.ElementAt(i-existindex), types1[i].Item2));

                    typekeys.Add(new KeyValuePair<string, Type>(types1[i].Item2, exists.ElementAt(i - existindex)));
                }
            }


            //var oiop = LatticeUtils.AnonymousTypeUtils.CreateType(typekeys);
            //return oiop;

            //int existindex = types.Count() - exists.Count();
            //foreach(var oo in types)
            //{
            //    tts.Add(Tuple.Create(oo.Item1, oo.Item2));
            //    if(--existindex <= 0)
            //    {
            //        break;
            //    }
            //}
            //foreach(var oo in exists)
            //{
            //    tts.Add(Tuple.Create(oo.Item1, oo.Item2));
            //}
            var fileds = tts.Select((x, i) =>
            {
                if (Type.GetTypeCode(x.Item1) == TypeCode.Object && x.Item1 != typeof(RegistryKey))
                {
                    //x.Item1.GetConstructors()[0].GetParameters().BuildType();
                }
                return tb.DefineField($"m_{x.Item2}", x.Item1, FieldAttributes.Private);
            }).ToList();
            
            Type[] parameterTypes = tts.Select(x => x.Item1).ToArray();

            var typeParameters = tb.DefineGenericParameters(tts.Select(x => $"T{x.Item2}").ToArray());

            ConstructorBuilder ctor1 = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);

            ILGenerator ctor1IL = ctor1.GetILGenerator();
            ctor1IL.Emit(OpCodes.Ldarg_0);
            ctor1IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            for (int i = 0; i < fileds.Count; i++)
            {
                ctor1IL.Emit(OpCodes.Ldarg_0);
                ctor1IL.Emit(OpCodes.Ldarg_S, i + 1);
                ctor1IL.Emit(OpCodes.Stfld, fileds[i]);
            }

            ctor1IL.Emit(OpCodes.Ret);


            foreach (var oo in fileds)
            {
                AddProperty(tb, oo, true, false);
            }


            return tb.CreateType();
        }

        public static Type BuildType(this IEnumerable<ParameterInfo> types)
        {
            return types.Select(x => Tuple.Create(x.ParameterType, x.Name)).BuildType(null);
        }

        public static Type BuildType(this IEnumerable<PropertyInfo> types)
        {
            return types.Select(x => Tuple.Create(x.PropertyType, x.Name)).BuildType(null);
        }
        static public int Replace(this Type[] datas, Type src, Type dst)
        {
            int count = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == src)
                {
                    datas[i] = dst;
                }
            }

            return count;
        }

        public static bool HaseRegistryKey(this Type src)
        {
            bool bb = false;
            if(src == typeof(RegistryKey))
            {
                return true;
            }
            else
            {
                var typecode = Type.GetTypeCode(src);
                if(typecode == TypeCode.Object)
                {

                    if(src.IsGenericType==true&&src.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        bb = src.GetGenericArguments()[0].HaseRegistryKey();
                    }
                    else if (src.IsGenericType == true && (src.GetGenericTypeDefinition() == typeof(Func<,>)|| src.GetGenericTypeDefinition() == typeof(Func<,,>)))
                    {
                        bb = src.GetGenericArguments().Last().HaseRegistryKey();
                    }
                    else if(src.IsGenericType==true&& src.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                    {
                        bb = src.GetGenericArguments().Any(x=>x==typeof(RegistryKey));
                    }
                    else
                    {
                        foreach (var oo in src.GetProperties())
                        {
                            bb = oo.PropertyType.HaseRegistryKey();
                            if(bb==true)
                            {
                                return true;
                            }
                        }
                    }
                    
                }
            }

            return bb;
        }

        public static IEnumerable<Tuple<Type, string>> Replace(this IEnumerable<ParameterInfo> datas, Type src, Type dst)
        {
            foreach(var oo in datas)
            {
                if(oo.ParameterType == src)
                {
                    yield return Tuple.Create(dst, oo.Name);
                }
                else
                {
                    yield return Tuple.Create(oo.ParameterType, oo.Name);
                }
            }
        }

        static public MethodInfo SelectMethod(this Type dst, Type src=null)
        {
            var select_method = typeof(Queryable).GetMethods().FirstOrDefault(x =>
            {
                bool hr = false;
                if (x.Name == "Select")
                {
                    var pps = x.GetParameters();
                    if (pps.Length == 2)
                    {
                        var ssss = pps[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length;
                        if (ssss == 2)
                        {
                            hr = true;
                        }
                    }
                }
                return hr;
            });
            MethodInfo method = null;
            if(src == null)
            {
                method = select_method?.MakeGenericMethod(typeof(RegistryKey), dst);
            }
            else
            {
                method = select_method?.MakeGenericMethod(src, dst);
            }
            return method;
        }

        static public MethodInfo SelectMethod_Enumerable(this Type datatype)
        {
            var select_method = typeof(Enumerable).GetMethods().FirstOrDefault(x =>
            {
                bool hr = false;
                if (x.Name == "Select")
                {
                    var pps = x.GetParameters();
                    if (pps.Length == 2)
                    {
                        var ssss = pps[1].ParameterType.GetGenericArguments().Length;
                        if (ssss == 2)
                        {
                            hr = true;
                        }
                    }
                }
                return hr;
            });
            return select_method?.MakeGenericMethod(typeof(RegistryKey), datatype);
        }

        static public Expression ToSelectData(this Type dst, ParameterExpression pp = null, Type src=null)
        {
            if (pp == null)
            {
                if(src == null)
                {
                    pp = Expression.Parameter(typeof(RegistryKey), "x");
                }
                else
                {
                    pp = Expression.Parameter(src, "x");
                }
            }
            Expression todata = null;
            if(src == null)
            {
                todata = dst.ToData(pp);
            }
            else
            {
                todata = dst.CopyData(src, pp);
            }
            if(todata == null)
            {
                return null;
            }
            var lambda = Expression.Lambda(todata, pp);
            UnaryExpression unary = null;
            if (src == null)
            {
                unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));
            }
            else
            {
                unary = Expression.MakeUnary(ExpressionType.Quote, lambda, src);
            }
            return unary;
        }

        public static Func<RegistryKey, TData> ToDataFunc<TData>(this RegistryKey reg)
        {
            var o_pp = Expression.Parameter(typeof(RegistryKey), "x");
            var o1 = typeof(TData).ToData(o_pp);
            var o_func = Expression.Lambda<Func<RegistryKey, TData>>(o1, o_pp).Compile();
            return o_func;
        }

        public static LambdaExpression ToLambdaData(this Type datatype, ParameterExpression pp = null)
        {
            if (pp == null)
            {
                pp = Expression.Parameter(typeof(RegistryKey), "z");
            }
            var todata = datatype.ToData(pp);
            var lambda = Expression.Lambda(todata, pp);
            return lambda;
        }

        static public Expression CopyData(this Type dst, Type src, Expression param)
        {
            if(dst.IsGenericType==true&& src == typeof(IEnumerable<RegistryKey>))
            {
                var sd = dst.GetGenericArguments()[0].ToLambdaData();
                var select_method = dst.GetGenericArguments()[0].SelectMethod_Enumerable();

                return Expression.Call(select_method, param, sd);
            }
            else
            {
                var dst_pps = dst.GetProperties();
                var dst_ccs = dst.GetConstructors();

                var src_pps = src.GetProperties();
                var src_ccs = src.GetConstructors();

                var pps = src_pps.Zip(dst_pps, (x, y) => new { src = x, dst = y });
                bool hasreg = false;
                List<Expression> exprs = new List<Expression>();
                foreach (var pp in pps)
                {
                    if (pp.src.PropertyType == typeof(RegistryKey))
                    {
                        var param1 = Expression.Property(param, pp.dst.Name);
                        var expr = pp.dst.PropertyType.ToData(param1);
                        var test = Expression.Equal(param1, Expression.Constant(null));
                        var ifelse = Expression.Condition(test, Expression.Constant(null, pp.dst.PropertyType), expr);
                        exprs.Add(ifelse);
                        hasreg = true;
                    }
                    else if (pp.src.PropertyType.IsGenericType == true && pp.src.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {

                    }
                    else if (pp.src.PropertyType.IsGenericType == true && pp.src.PropertyType == typeof(IEnumerable<RegistryKey>))
                    {
                        var param1 = Expression.Property(param, pp.dst.Name);
                        var ppp = param1.Expression as ParameterExpression;
                        var sd = pp.dst.PropertyType.GetGenericArguments()[0].ToLambdaData();
                        var select_method = pp.dst.PropertyType.GetGenericArguments()[0].SelectMethod_Enumerable();
                        var aaaaa = Expression.Call(select_method, param1, sd);
                        exprs.Add(aaaaa);
                        hasreg = true;
                    }
                    else if (pp.src.PropertyType.IsGenericType == true && pp.src.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)&& pp.src.PropertyType==typeof(IEnumerable<RegistryKey>))
                    {
                        var param1 = Expression.Property(param, pp.dst.Name);
                        var ppp = param1.Expression as ParameterExpression;
                        var sd = pp.dst.PropertyType.GetGenericArguments()[0].ToLambdaData();
                        var select_method = pp.dst.PropertyType.GetGenericArguments()[0].SelectMethod_Enumerable();
                        var aaaaa = Expression.Call(select_method, param1, sd);
                        exprs.Add(aaaaa);
                    }
                    else if (Type.GetTypeCode(pp.src.PropertyType) == TypeCode.Object)
                    {
                        var expr = CopyData(pp.dst.PropertyType, pp.src.PropertyType, Expression.Property(param, pp.dst.Name));
                        if (expr == null)
                        {
                            expr = Expression.Property(param, pp.dst.Name);
                        }
                        exprs.Add(expr);
                    }
                    else
                    {
                        var param1 = Expression.Property(param, pp.dst.Name);
                        exprs.Add(param1);
                    }
                }
                Expression memberinit = null;
                if (hasreg == true)
                {
                    memberinit = Expression.New(dst_ccs[0], exprs, dst.GetProperties());
                }
                return memberinit;
            }
            
        }

        static public Expression ToData(this Type dst, Expression param)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
            var pps = dst.GetProperties().Where(x => x.CanWrite == true && x.GetCustomAttributes(typeof(RegIgnore), true).Length == 0);
            var ccs = dst.GetConstructors();
            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var pp in pps)
            {
                var regattr = pp.GetCustomAttributes(true).FirstOrDefault();
                Expression name = null;
                if (regattr != null && regattr is RegSubKeyName)
                {
                    RegSubKeyName subkeyname = regattr as RegSubKeyName;
                    Expression mm = null;
                    mm = Expression.Property(param, "Name");
                    if (subkeyname.IsFullName == false)
                    {
                        var method = typeof(RegQueryHelper).GetMethod("GetLastSegement");
                        mm = Expression.Call(method, mm);
                    }
                    var binding = Expression.Bind(pp, mm);
                    bindings.Add(binding);
                }
                else if (regattr != null && regattr is RegPropertyName)
                {
                    var reganme = regattr as RegPropertyName;
                    name = Expression.Constant(reganme.Name, typeof(string));
                }
                else
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                }
                if (name != null)
                {
                    if (pp.PropertyType.IsGenericTypeDefinition == true && pp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                        UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
                        var binding = Expression.Bind(pp, unary1);
                        bindings.Add(binding);
                    }
                    else
                    {
                        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                        var binding = Expression.Bind(pp, method);
                        bindings.Add(binding);
                    }
                }

            }

            var memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);

            return memberinit;
        }


        public static Type Find(this Type src, MethodInfo method, string target)
        {
            var args1 = method.GetGenericMethodDefinition().GetGenericArguments().Select(x => x.Name);
            var args2 = method.GetGenericMethodDefinition().GetParameters().ToList();
            var args = args2[1].ParameterType.GetGenericArguments();
            var args_1 = src.GetGenericArguments();
            foreach (var oo in args)
            {
                var define = oo.GetGenericTypeDefinition();
                if (define == typeof(Func<,>) || define == typeof(Func<,,>))
                {
                    var gg = oo.GetGenericArguments().Select(x => x.Name);
                    var tty = oo.GetGenericArguments()[1];
                    if(tty == typeof(IEnumerable<>))
                    {

                    }
                    var aaa = tty.GetGenericArguments();
                }

                
            }
            foreach (var oo in args_1)
            {
                var define = oo.GetGenericTypeDefinition();
                if (define == typeof(Func<,>) || define == typeof(Func<,,>))
                {
                    var gg = oo.GetGenericArguments().Select(x => x.Name);
                    var tty = oo.GetGenericArguments()[1];
                    if (tty == typeof(IEnumerable<>))
                    {

                    }
                    var aaa = tty.GetGenericArguments();
                }
            }

            return null;
        }

        static Type Findd(this string src, Type expr, Type method)
        {
            Type result = null;
            if(method.GetGenericArguments().Length ==0 && method.Name == src)
            {
                return expr;
            }
            var zip = expr.GetGenericArguments().Zip(method.GetGenericArguments(), (ee, mm) => new { ee, mm });
            foreach (var oo in zip)
            {
                if (oo.mm.Name == src)
                {
                    return oo.ee;
                }
                else
                {
                    result = src.Findd(oo.ee, oo.mm);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        public static Type ChangeType(this Type src, Dictionary<Type, Type> changes)
        {
            if(src.IsGenericType == true)
            {
                var args = src.GetGenericArguments();
                for(int i=0; i<args.Length; i++)
                {
                    if(args[i].IsGenericType == true)
                    {
                        if (changes.ContainsKey(args[i]))
                        {
                            args[i] = changes[args[i]];
                        }
                        args[i] = args[i].ChangeType(changes);
                    }
                    else
                    {
                        if (changes.ContainsKey(args[i]))
                        {
                            args[i] = changes[args[i]];
                        }
                    }

                }
                return src.GetGenericTypeDefinition().MakeGenericType(args);
            }
            else
            {
                if(changes.ContainsKey(src))
                {
                    return changes[src];
                }
                return src;
            }
        }

        public static Type[] GetTypes(this IEnumerable<Expression> src, MethodInfo method)
        {
            if (method.IsGenericMethod == true)
            {
                var method_define = method.GetGenericMethodDefinition();
                var names = method_define.GetGenericArguments().Select(x => x.Name).ToArray();

                var method_pps = method_define.GetParameters();

                Type[] types1 = new Type[names.Length];
                for (int i = 0; i < types1.Length; i++)
                {
                    for (int j = 0; j < method_pps.Length; j++)
                    {
                        //if(method_pps[j].ParameterType.IsGenericType == false)
                        //{
                        //    types1[i] = method_pps[j].ParameterType;
                        //}
                        //else
                        {
                            types1[i] = names.ElementAt(i).Findd(src.ElementAt(j).Type, method_pps[j].ParameterType);
                        }
                        if (types1[i] != null)
                        {
                            break;
                        }
                    }


                }
                return types1;
            }
            else
            {
                return src.Select(x => x.Type).ToArray();
            }
        }

        public static Dictionary<string,Type> GetTypess(this IEnumerable<Expression> src, MethodInfo method)
        {
            if (method.IsGenericMethod == true)
            {
                var method_define = method.GetGenericMethodDefinition();
                var names = method_define.GetGenericArguments().Select(x => x.Name).ToArray();

                var method_pps = method_define.GetParameters();
                var types1 = new Dictionary<string, Type>();


                for (int i = 0; i < names.Length; i++)
                {
                    for (int j = 0; j < method_pps.Length; j++)
                    {
                        var ttype = names.ElementAt(i).Findd(src.ElementAt(j).Type, method_pps[j].ParameterType);
                        if (ttype != null)
                        {
                            types1[names.ElementAt(i)] = ttype;
                            break;
                        }
                    }


                }
                return types1;
            }
            else
            {
                return src.ToDictionary(x => x.Type.Name, x => x.Type);
               // return src.Select(x => x.Type).ToArray();
            }
        }


        public static string GetLastSegement(this string src)
        {
            string basekeyname = "";
            string path = "";
            Regex regex1 = new Regex(@"^(?<base>\w+)[\\](?<path>.+)[\\]", RegexOptions.Compiled);
            //Regex regex2 = new Regex(@"^(?<base>\w+)[\\]", RegexOptions.Compiled);
            var match = regex1.Match(src);
            if (match.Success == true)
            {
                basekeyname = match.Groups["base"].Value;
                path = match.Groups["path"].Value;
            }
            else
            {
                basekeyname = src;
            }
            var segement = src.Replace(basekeyname, "").Replace(path, "").TrimStart('\\');
            return segement;
        }

        static public List<string> FindAllGeneric(this Type src)
        {
            List<string> types = new List<string>();
            if(src.IsGenericParameter == true)
            {
                types.Add(src.Name);
                
            }
            else if(src.IsGenericType == true)
            {
                var pps = src.GetGenericArguments();

                if (pps.Length == 0)
                {
                    types.Add(src.Name);
                }
                else
                {
                    foreach (var oo in pps)
                    {
                        if (oo.IsGenericParameter == true)
                        {
                            types.AddRange(oo.FindAllGeneric());
                        }
                        else
                        {
                            types.Add(oo.Name);
                        }
                    }
                }
            }
            else
            {
                types.Add(src.Name);
            }
            return types;
        }

        static public RegistryKey GetParent(this RegistryKey src)
        {
            string basekeyname = "";
            string path = "";
            Regex regex1 = new Regex(@"^(?<base>\w+)[\\](?<path>.+)[\\]", RegexOptions.Compiled);
            //Regex regex2 = new Regex(@"^(?<base>\w+)[\\]", RegexOptions.Compiled);
            var match = regex1.Match(src.Name);
            if (match.Success == true)
            {
                basekeyname = match.Groups["base"].Value;
                path = match.Groups["path"].Value;
            }
            else
            {
                basekeyname = src.Name;
            }
            RegistryHive hive;
            switch (basekeyname)
            {
                case "HKEY_CURRENT_USER":
                    {
                        hive = RegistryHive.CurrentUser;
                    }
                    break;
                case "HKEY_CLASSES_ROOT":
                    {
                        hive = RegistryHive.ClassesRoot;
                    }
                    break;
                case "HKEY_LOCAL_MACHINE":
                    {
                        hive = RegistryHive.LocalMachine;
                    }
                    break;
                case "HKEY_CURRENT_CONFIG":
                    {
                        hive = RegistryHive.CurrentConfig;
                    }
                    break;
                case "HKEY_DYN_DATA":
                    {
                        hive = RegistryHive.DynData;
                    }
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    {
                        hive = RegistryHive.PerformanceData;
                    }
                    break;
                case "HKEY_USERS":
                    {
                        hive = RegistryHive.Users;
                    }
                    break;
                default:
                    {
                        throw new Exception("RegistryHive not find");
                    }
                    break;
            }
            var basekey = RegistryKey.OpenBaseKey(hive, src.View);
            var reg = basekey.OpenSubKey(path, true);
            basekey.Close();
            basekey.Dispose();
            return reg;
        }
    }
}
