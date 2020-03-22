using ADO.Utils;

namespace ADO.Models
{
    public class BaseViewModel
    {
        public override string ToString() => JsonUtilities.SerializeObject(this);
    }
}
