using System;

namespace DNA.CastleMinerZ.ModAPI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModAttribute : Attribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
