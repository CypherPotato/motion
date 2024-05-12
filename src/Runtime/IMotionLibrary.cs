using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime;

public interface IMotionLibrary
{
    public string? Namespace { get; }
    public void ApplyMembers(ExecutionContext context);
}
