using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
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
