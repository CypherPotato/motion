using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;

internal class StdRandom : IMotionLibrary
{
    public string? Namespace => "random";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("next", Next);
        context.Methods.Add("string", String);
        context.Methods.Add("guid", () => Guid.NewGuid());
    }

    string String(Atom self)
    {
        string alphabet = "";

        if (self.HasKeyword("lower")) alphabet += "abcdefghijklmnopqrstuvwxyz";
        if (self.HasKeyword("upper")) alphabet += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (self.HasKeyword("digits")) alphabet += "0123456789";
        if (self.HasKeyword("symbols")) alphabet += "+-_!@#$%&";

        if (alphabet == "")
        {
            throw new InvalidOperationException("alphabet is empty");
        }

        int length = 16;

        if (self.ItemCount == 2)
        {
            length = self.GetAtom(1).GetInt32();
        }
        else if (self.ItemCount != 1)
        {
            throw new InvalidOperationException("rand:string expects zero or one parameter only.");
        }

        return new string(Enumerable.Repeat(alphabet, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    object Next(Atom self)
    {
        if (self.HasKeyword("double"))
            return NextDouble(self);

        if (self.ItemCount == 1)
        {
            return Random.Shared.Next();
        }
        else if (self.ItemCount == 2)
        {
            int max = self.GetAtom(1).GetInt32();
            return Random.Shared.Next(max);
        }
        else if (self.ItemCount == 3)
        {
            int min = self.GetAtom(1).GetInt32();
            int max = self.GetAtom(2).GetInt32();
            return Random.Shared.Next(min, max);
        }
        throw new InvalidOperationException("rand:next expects zero, one or three parameters only.");
    }

    double NextDouble(Atom self)
    {
        if (self.ItemCount == 1)
        {
            return Random.Shared.NextDouble();
        }
        else if (self.ItemCount == 2)
        {
            double max = self.GetAtom(1).GetDouble();
            return Random.Shared.NextDouble() * max;
        }
        else if (self.ItemCount == 3)
        {
            double min = self.GetAtom(1).GetDouble();
            double max = self.GetAtom(2).GetDouble();
            return Random.Shared.NextDouble() * max + min;
        }
        throw new InvalidOperationException("rand:next expects zero, one or three parameters only.");
    }
}
