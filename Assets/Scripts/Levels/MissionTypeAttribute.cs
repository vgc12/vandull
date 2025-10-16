using System;

namespace Levels
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MissionTypeAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        
        public MissionTypeAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}