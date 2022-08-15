using System.Collections;
using UnityEngine;

namespace CustomUtilities
{
    public static class DamageUtilities
    {

        public static bool HasType(this DamageType type, DamageType check)
        {
            return (type & check) != 0;
        }
    }
}