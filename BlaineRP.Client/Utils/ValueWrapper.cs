﻿namespace BlaineRP.Client.Utils
{
    public class RefValue<T> where T : struct
    {
        public T Value { get; set; }

        public RefValue() { }

        public RefValue(T value)
        {
            this.Value = value;
        }

        public static implicit operator T(RefValue<T> wrapper)
        {
            if (wrapper == null)
            {
                return default(T);
            }

            return wrapper.Value;
        }
    }
}
