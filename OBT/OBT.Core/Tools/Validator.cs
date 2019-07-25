using System;

namespace OBT.Core.Tools
{
    public static class Validator
    {
        public static void ThrowIfNull(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
