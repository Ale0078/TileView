using System;

namespace TileView.Extentions
{
    public static class TypeChecker
    {
        public static bool DoesMatchType(this object objectToCheckType, Type typeToChecking) 
        {
            if (objectToCheckType is null || objectToCheckType.GetType().Equals(typeToChecking))
            {
                return true;
            }

            return false;
        }
    }
}
