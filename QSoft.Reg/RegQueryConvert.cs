using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSoft.Registry.Linq
{
    public abstract class RegQueryConvert
    {
        internal RegQueryConvert() { }
        abstract public bool CanConvert(Type src);
        abstract public Type GetSourceType();
    }

    public abstract class RegQueryConvert<TSrc, TDst> : RegQueryConvert where TDst : struct
    {
        public override bool CanConvert(Type src)
        {
            return src == typeof(TSrc);
        }
        //protected Type Src { get; } = typeof(TSrc);
        abstract public TDst ConvertTo(TSrc src);
        abstract public TSrc ConvertBack(TDst dst);
        override public Type GetSourceType()
        {
            return typeof(TSrc);
        }
    }

    public abstract class RegQueryConvert<TSrc> : RegQueryConvert
    {
        public override bool CanConvert(Type src)
        {
            return src == typeof(TSrc);
        }
        //protected Type Src { get; } = typeof(TSrc);
        abstract public string ConvertTo(TSrc src);
        abstract public TSrc ConvertBack(string dst);
        override public Type GetSourceType()
        {
            return typeof(TSrc);
        }
    }
}

namespace QSoft.Registry.Linq.Convert
{
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
}
