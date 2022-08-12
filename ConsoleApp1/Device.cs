using QSoft.Registry.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    public class Device
    {
        ////public Device() { }
        ////public Device(string name) { }
        ////[RegPropertyName("LocalIP")]
        ////public Address Local { set; get; }
        ////[RegPropertyName("RemoteIP")]
        ////public Address Remote { set; get; }
        ////public string Name { set; get; }
        ////public int? ID { set; get; }
        ////public Size Size { set; get; }
        ////public Locationata Location { set; get; }
        ////public CameraSetting CameraSetting { set; get; }
        //[RegPropertyName("Ver")]
        //public Version Version { set; get; }

        [RegPropertyName("LocalIP")]
        public Address Local { set; get; }
        [RegPropertyName("RemoteIP")]
        public Address Remote { set; get; }
        public string Name { set; get; }
        public Size Size { set; get; }
        public Locationata Location { set; get; }
        public CameraSetting CameraSetting { set; get; }
        [RegSubKeyName]
        public string Key { set; get; }
    }

    public class Locationata
    {
        public string Name { set; get; }
        public FloorData Floor { set; get; }
    }

    public class FloorData
    {
        public string Name { set; get; }
        public int Floor { set; get; }
        public AreaData Area { set; get; }
    }

    public class AreaData
    {
        public string Name { set; get; }
        public Rect Data { set; get; }
    }

    public class Point
    {
        public int X { set; get; }
        public int Y { set; get; }
    }

    public class Size
    {
        public int Width { set; get; }
        public int Height { set; get; }
    }

    public class Rect
    {
        public Point Point { set; get; }
        public Size Size { set; get; }
    }

    public class CameraSetting
    {
        public Brightness Brightness { set; get; }
        public PIR PIR { set; get; }
        public WDR WDR { set; get; }
    }

    public class PIR
    {
        public bool IsEnable { set; get; }
        public bool IsAuto { set; get; }
    }

    public class WDR
    {
        public bool IsEnable { set; get; }
        public int Level { set; get; }
    }

    public class Brightness
    {
        public int Current { set; get; }
        public Range Range { set; get; }
        public bool CanEdit { set; get; }
    }

    public class Range
    {
        public double Max { set; get; }
        public double Min { set; get; }
    }

    public class Address
    {
        public string IP { set; get; }
        public int? Port { set; get; }
        public Auth Root { set; get; }
        public Auth Guest { set; get; }
        public class Auth
        {
            public string Account { set; get; }
            public string Password { set; get; }
        }
    }
}
