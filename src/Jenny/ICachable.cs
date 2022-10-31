using System.Collections.Generic;

namespace Jenny
{
    public interface ICachable
    {
        Dictionary<string, object> ObjectCache { get; set; }
    }
}
