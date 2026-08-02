using System.Collections.Generic;

namespace PosCore.Models
{
    public class IndustryProfile
    {
        public string IndustryName { get; set; }
        public List<ShortcutConfig> Shortcuts { get; set; }
    }
}
