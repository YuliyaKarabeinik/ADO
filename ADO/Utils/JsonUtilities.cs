using Newtonsoft.Json;

namespace ADO.Utils
{
    class JsonUtilities
    {
        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings 
                    {NullValueHandling = NullValueHandling.Ignore});
        }
    }
}
