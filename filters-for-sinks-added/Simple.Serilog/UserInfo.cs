using System.Collections.Generic;

namespace Simple.Serilog
{
    public class UserInfo
    {
        public string Name { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }
}
