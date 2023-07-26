namespace BlaineRP.Client.Utils
{
    public class RefValue<T> where T : struct
    {
        public RefValue()
        {
        }

        public RefValue(T value)
        {
            Value = value;
        }

        public T Value { get; set; }

        public static implicit operator T(RefValue<T> wrapper)
        {
            if (wrapper == null)
                return default(T);

            return wrapper.Value;
        }
    }
}