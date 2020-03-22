using System;
using System.Data.Common;

namespace ADO
{
    static class SQLUtilities
    {
        public static T GetSafe<T>(this DbDataReader reader, string columnName)
        {
            var columnIndex = reader.GetOrdinal(columnName);
            if (typeof(T).IsEnum)
            {
               return GetSafeEnumValue<T>(reader[columnName]);
            }

            var returnValue = reader.IsDBNull(columnIndex) ? default(T) : reader.GetFieldValue<T>(columnIndex);
            return returnValue;
        }

        private static T GetSafeEnumValue<T>(object o)
        {
            T enumVal = (T)Enum.Parse(typeof(T), o.ToString());
            return enumVal;
        }
    }
}
