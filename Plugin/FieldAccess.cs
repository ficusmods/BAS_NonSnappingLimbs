using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace NonSnappingLimbs
{
    public static class FieldAccess
    {
        public static bool Read<T>(object obj, string fieldName, out T output)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            if(field != null)
            {
                output = (T)field.GetValue(obj);
                return true;
            }
            output = default(T);
            return false;
        }

        public static bool Write<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName,
             System.Reflection.BindingFlags.Public
             | System.Reflection.BindingFlags.NonPublic
             | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            return false;
        }

        public static bool WriteProperty<T>(object obj, string fieldName, T value)
        {
            var prop = obj.GetType().GetProperty(fieldName,
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);

            if (prop != null)
            {
                prop.SetValue(obj, value);
                return true;
            }
            return false;
        }
    }
}
