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

            return reader.IsDBNull(columnIndex) ? default(T) : reader.GetFieldValue<T>(columnIndex);            
        }

        public static DateTime? GetNullableDateTime(this DbDataReader reader, string columnName)
        {
            var columnIndex = reader.GetOrdinal(columnName);
            return reader.IsDBNull(columnIndex) ? (DateTime?) null : reader.GetFieldValue<DateTime>(columnIndex);
        }

        public static int? GetNullableInt(this DbDataReader reader, string columnName)
        {
            var columnIndex = reader.GetOrdinal(columnName);
            return reader.IsDBNull(columnIndex) ? (int?)null : reader.GetFieldValue<int>(columnIndex);
        }

        private static T GetSafeEnumValue<T>(object o)
        {
            T enumVal = (T)Enum.Parse(typeof(T), o.ToString());
            return enumVal;
        }
    }
}
