using System.Collections.Generic;
using System.Linq;

namespace Community.PowerToys.Run.Plugin.TailwindCSS.Models;

public class Hierarchy : Dictionary<string, string?>
{
    public override string ToString()
    {
        return string.Join(" / ", this.Select(x => x.Value).Where(x => string.IsNullOrWhiteSpace(x) == false));
    }

}
