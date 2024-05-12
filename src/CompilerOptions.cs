using Motion.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion;

public class CompilerOptions
{
    public bool AllowDotNetInvoke { get; set; } = false;
    public ICollection<IMotionLibrary> Libraries { get; set; } = new List<IMotionLibrary>();
}
