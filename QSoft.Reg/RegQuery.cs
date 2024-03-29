﻿using Microsoft.Win32;
using QSoft.Registry.Linq.Convert;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace QSoft.Registry.Linq
{
    public class RegQuery<T> : IOrderedQueryable<T>
    {
        public RegQuery()
        {
            this.Expression = Expression.Constant(this);
            this.m_Provider.Converts = this.m_Converts;
        }

        RegSetting m_Setting = new RegSetting();
        RegProvider<T> m_Provider = new RegProvider<T>();
        public RegQuery<T> useSetting(Action<RegSetting> data)
        {
            data(this.m_Provider.Setting);
            data(this.m_Setting);
            //m_Setting = provider.Setting;
            this.Provider = this.m_Provider;
            return this;
        }

        Dictionary<Type, RegQueryConvert> m_Converts = new Dictionary<Type, RegQueryConvert>()
        {
            {typeof(Version), new Version2String() }
        };

        public RegQuery<T> useConverts(Action<List<RegQueryConvert>> converts)
        {
            List<RegQueryConvert> convert = new List<RegQueryConvert>();
            converts(convert);
            foreach(var oo in convert)
            {
                this.m_Converts[oo.GetSourceType()] = oo;
            }

            this.m_Provider.Converts = this.m_Converts;
            return this;
        }

        public RegQuery<T> HasDefault1(Action<T> data)
        {
            //var obj = Activator.CreateInstance<T>();
            //data(obj);
            //System.Threading.Thread.Sleep(2000);
            //data(obj);
            this.m_Provider.DefaultValue = data;

            return this;
        }

        public RegQuery<T> HasDefault(Expression<Func<T>> data)
        {
            return this;
        }
        public Expression Expression_Reg { private set; get; }
        public RegQuery(IQueryProvider provider, Expression expression, Expression expressionreg)
        {
            this.Provider = provider;
            this.Expression = expression;
            this.Expression_Reg = expressionreg;
        }

        //public RegQuery(IQueryProvider provider, Expression expression)
        //{
        //    this.Provider = provider;
        //    this.Expression = expression;
        //}
#if TestProvider
        public Expression Expression_Reg { private set; get; }
        public RegQuery(IQueryProvider provider, Expression src, Expression dst)
        {
            this.Provider = provider;
            this.Expression = src;
            this.Expression_Reg = dst;
        }
#endif

        Type m_ElementType = typeof(RegistryKey);
        Expression m_Expression;
        public Expression Expression { private set
            {
                m_Expression = value;
            }
            get 
            {
                return m_Expression;
            } }
        public Type ElementType => typeof(RegistryKey);
        public IQueryProvider Provider { private set; get; }
        public IEnumerator<T> GetEnumerator()
        {
            var provider = this.Provider as RegProvider<T>;
            if(provider != null)
            {
                var fail = provider.CheckFail();
                if (fail != null)
                {
                    throw fail;
                }
                //return provider.Execute<IEnumerable<T>>(provider.Expr_Dst).GetEnumerator();
            }
#if TestProvider
            return Provider.Execute<IEnumerable<T>>(Expression_Reg).GetEnumerator();
#else
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
#endif
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }





        [Obsolete("Testing", true)]
        public void Create(T data, bool isoverwrite=false)
        {
            var setting = (this.Provider as RegProvider<T>)?.Setting;
            RegistryKey reg = setting?.Open(true);
            if(reg != null && isoverwrite==false)
            {
                throw new Exception("Exist RegsistryKey");
            }
            else if(reg != null)
            {
                var names = reg.GetValueNames();
                foreach(var oo in names)
                {
                    reg.DeleteValue(oo);
                }
            }
            else
            {
                reg = setting?.Create(true);
            }
            var pps = this.DumpPropertys(data);
            foreach(var pp in pps)
            {
                var oo = pp.Key.GetValue(data, null);
                reg.SetValue(pp.Value, oo);
            }

        }

        Dictionary<PropertyInfo, string> DumpPropertys(T data)
        {
            var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
            Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            foreach (var pp in pps)
            {
                var attr = pp.GetCustomAttributes(true).FirstOrDefault();
                string ppname = "";
                if (attr is RegIgnore)
                {

                }
                else if (attr is RegPropertyName)
                {
                    ppname = (attr as RegPropertyName)?.Name;
                }
                else if (attr is RegSubKeyName)
                {

                }
                else
                {
                    ppname = pp.Name;
                }
                if (string.IsNullOrEmpty(ppname) == false)
                {
                    dicpps[pp] = ppname;
                    var regnames = pp.GetCustomAttributes(typeof(RegPropertyName), true) as RegPropertyName[];
                    if (regnames.Length > 0)
                    {
                        dicpps[pp] = regnames[0].Name;
                    }
                }

            }
            return dicpps;
        }
        //[Obsolete("Testing", true)]
        //public void Update(T data)
        //{
        //    var setting = (this.Provider as RegProvider<T>)?.Setting;
        //    RegistryKey reg = setting?.Open(true);
        //    var pps = this.DumpPropertys(data);
        //    foreach (var pp in pps)
        //    {
        //        var oo = pp.Key.GetValue(data, null);
        //        reg.SetValue(pp.Value, oo);
        //    }
        //}
        //[Obsolete("Testing", true)]
        //public T Get()
        //{
        //    RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Open();
        //    var func_todata = reg.ToDataFunc<T>(this.m_Converts);
        //    return func_todata(reg);
        //    //return reg.ToDataFunc<T>()(reg);
        //}
        //[Obsolete("Testing", true)]
        //public void Delete()
        //{
        //    RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Open();
        //    var parent = reg.GetParent();
        //    Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
        //    var match = regex1.Match(reg.Name);
        //    if (match.Success)
        //    {
        //        parent.DeleteSubKeyTree(match.Groups["path"].Value);
        //    }
        //}

        void Admin(string se)
        {
            IntPtr token;
            NativeMethod.OpenProcessToken(NativeMethod.GetCurrentProcess(), NativeMethod.TOKEN_ALL_ACCESS, out token);
            NativeMethod.LUID PrivilegeRequired = new NativeMethod.LUID();
            bool bRes = false;
            bRes = NativeMethod.LookupPrivilegeValue(null, se, ref PrivilegeRequired);
            
            NativeMethod.TOKEN_PRIVILEGES tp = new NativeMethod.TOKEN_PRIVILEGES();
            tp.Privileges = new NativeMethod.LUID_AND_ATTRIBUTES[1];
            tp.PrivilegeCount = 1;
            tp.Privileges[0].Luid = PrivilegeRequired;
            tp.Privileges[0].Attributes = NativeMethod.SE_PRIVILEGE_ENABLED;
            NativeMethod.TOKEN_PRIVILEGES tp1 = new NativeMethod.TOKEN_PRIVILEGES();
            uint hr;
            bRes = NativeMethod.AdjustTokenPrivileges(token, false, ref tp, (uint)Marshal.SizeOf(tp), ref tp1, out hr);

            NativeMethod.CloseHandle(token);
        }

        string ErrMsg(int data)
        {
            //LPTSTR errorText = NULL;

            //FormatMessage(
            //    // use system message tables to retrieve error text
            //    FORMAT_MESSAGE_FROM_SYSTEM
            //    // allocate buffer on local heap for error text
            //    | FORMAT_MESSAGE_ALLOCATE_BUFFER
            //    // Important! will fail otherwise, since we're not 
            //    // (and CANNOT) pass insertion parameters
            //    | FORMAT_MESSAGE_IGNORE_INSERTS,
            //    NULL,    // unused with FORMAT_MESSAGE_FROM_SYSTEM
            //    data,
            //    MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
            //    (LPTSTR) & errorText,  // output 
            //    0, // minimum size for output buffer
            //    NULL);   // arguments - see note 
            StringBuilder strb = new StringBuilder(234);
            int hr = NativeMethod.FormatMessage(NativeMethod.FORMAT_MESSAGE.FROM_SYSTEM | NativeMethod.FORMAT_MESSAGE.ALLOCATE_BUFFER | NativeMethod.FORMAT_MESSAGE.IGNORE_INSERTS, IntPtr.Zero, data, 0, out strb, strb.Capacity, IntPtr.Zero);
            return strb.ToString();
        } 

        public void Backup(string filename, bool overwrite=true)
        {
            int ret = 0;
            IntPtr hKey = IntPtr.Zero;
            NativeMethod.RegistryDispositionValue dwDisposition;
            try
            {
                if (File.Exists(filename) == true)
                {
                    if(overwrite == false)
                    {
                        throw new Exception("File is Existed");
                    }
                    else
                    {
                        File.Delete(filename);
                    }
                }
                Admin("SeBackupPrivilege");
                ret = NativeMethod.RegCreateKeyEx((uint)this.m_Setting.Hive, this.m_Setting.SubKey, 0, null, 4, 0, IntPtr.Zero, out hKey, out dwDisposition);
                if (ret != 0)
                {
                    var err = ErrMsg(ret);
                    throw new Exception(err);
                }
                else
                {
                    int is_success = NativeMethod.RegSaveKeyEx(hKey, filename, IntPtr.Zero, 1);
                    if(is_success != 0)
                    {
                        var err = ErrMsg(is_success);
                        throw new Exception(err);
#pragma warning disable CS0162 // 偵測到不會執行到的程式碼
                        err = "";
#pragma warning restore CS0162 // 偵測到不會執行到的程式碼
                    }
                    
                    //NativeMethod.RegCloseKey();
                }
            }
#pragma warning disable CS0168 // 已宣告變數 'ee'，但從未使用過它。
            catch(Exception ee)
#pragma warning restore CS0168 // 已宣告變數 'ee'，但從未使用過它。
            {
                throw;
            }
            finally
            {
                if(hKey != IntPtr.Zero)
                {
                    NativeMethod.RegCloseKey(hKey);
                }
            }
        }

        public void Restore(string filename)
        {
            if (File.Exists(filename) == false)
            {
                throw new Exception("File is not Existed");
            }
            IntPtr hKey = IntPtr.Zero;
            try
            {
                Admin("SeRestorePrivilege");
                NativeMethod.RegistryDispositionValue dwDisposition;
                
                int ret;
                //const wchar_t* hiveName = L"Citys1";
                //const wchar_t* filename = L"test";
                ret = NativeMethod.RegCreateKeyEx((uint)this.m_Setting.Hive, this.m_Setting.SubKey, 0, null, 4, 0, IntPtr.Zero, out hKey, out dwDisposition);

                if (ret != 0)
                {
                    var err = ErrMsg(ret);
                    throw new Exception(err);
                }
                //else if (dwDisposition != REG_OPENED_EXISTING_KEY)
                //{
                //    RegCloseKey(hKey);
                //}
                else
                {
                    int is_success = NativeMethod.RegRestoreKey(hKey, filename, 8);
                    if(is_success != 0)
                    {
                        var err = ErrMsg(ret);
                        throw new Exception(err);
                    }
                }
            }
#pragma warning disable CS0168 // 已宣告變數 'ee'，但從未使用過它。
            catch(Exception ee)
#pragma warning restore CS0168 // 已宣告變數 'ee'，但從未使用過它。
            {
                throw;
            }
            finally
            {
                if (hKey != IntPtr.Zero)
                {
                    NativeMethod.RegCloseKey(hKey);
                }
            }
        }

        public RegistryKey ToRegistryKey()
        {
            RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Create(true);
            return reg;
        }
    }

    public class EqualityComparerForce<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            var pps = typeof(T).GetProperties().Where(p => p.CanRead == true);
            foreach (var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.PropertyType);
                dynamic s1 = pp.GetValue(x, null);
                dynamic s2 = pp.GetValue(y, null);
                if(s1 != s2)
                {
                    return false;
                }
                
            }
            return true;
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
