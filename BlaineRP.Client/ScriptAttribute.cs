using System;

namespace BlaineRP.Client
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptAttribute : Attribute
    {
        public readonly int LoadOrder;

        public ScriptAttribute(int loadOrder = int.MaxValue)
        {
            this.LoadOrder = loadOrder;
        }
    }
}
