using System;

namespace BlaineRP.Server.Language
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    internal class LocalizedAttribute : Attribute
    {
        public readonly string GlobalKey;
        public readonly string LocalKey;

        public LocalizedAttribute(string globalKey, string localKey = null)
        {
            GlobalKey = globalKey;
            LocalKey = localKey;
        }
    }
}