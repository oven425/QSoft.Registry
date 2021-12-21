using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSoft.Registry.Linq
{
    public class RegSetting
    {
        public string SubKey { set; get; }
        public RegistryHive Hive { set; get; }
        public RegistryView View { set; get; }

        public RegistryKey Open(bool write = false)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(this.Hive, this.View);
            if (string.IsNullOrEmpty(this.SubKey) == false)
            {
                RegistryKey reg = reg_base.OpenSubKey(this.SubKey, write);
                reg_base.Dispose();
                return reg;
            }
            return reg_base;
        }

        public RegistryKey Create(bool write = false)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(this.Hive, this.View);
            if (string.IsNullOrEmpty(this.SubKey) == false)
            {
                RegistryKey reg = reg_base.CreateSubKey(this.SubKey, write==true?RegistryKeyPermissionCheck.ReadWriteSubTree: RegistryKeyPermissionCheck.ReadSubTree);
                reg_base.Dispose();
                return reg;
            }

            return reg_base;
        }
    }
}
