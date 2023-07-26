namespace BlaineRP.Client.Extensions.System
{
    public static class TypeExtensions
    {
        public static bool IsTypeOrAssignable(this global::System.Type bType, global::System.Type type)
        {
            return bType == type || bType.IsAssignableFrom(type);
        }
    }
}