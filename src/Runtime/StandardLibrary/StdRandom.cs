using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Motion.Runtime.StandardLibrary;
internal class StdRandom : IMotionLibrary
{
    public string? Namespace => "random";

    public void ApplyMembers(ExecutionContext context)
    {
        context.Methods.Add("next", atom =>
        {
            if (atom.ItemCount == 1)
            {
                return Random.Shared.Next();
            }
            else if (atom.ItemCount == 2)
            {
                int max = atom.GetAtom(1).GetInt32();
                return Random.Shared.Next(max);
            }
            else if (atom.ItemCount == 3)
            {
                int min = atom.GetAtom(1).GetInt32();
                int max = atom.GetAtom(2).GetInt32();
                return Random.Shared.Next(min, max);
            }
            throw new InvalidOperationException("rand:next expects zero, one or three parameters only.");
        });
        context.Methods.Add("next-double", atom =>
        {
            atom.EnsureExactItemCount(1);
            return Random.Shared.NextDouble();
        });
        context.Methods.Add("guid", atom =>
        {
            atom.EnsureExactItemCount(1);
            return Guid.NewGuid().ToString();
        });
        context.Methods.Add("string", atom =>
        {
            string alphabet = "";

            if (atom.HasKeyword("lower")) alphabet += "abcdefghijklmnopqrstuvwxyz";
            if (atom.HasKeyword("upper")) alphabet += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (atom.HasKeyword("digits")) alphabet += "0123456789";
            if (atom.HasKeyword("symbols")) alphabet += "+-_!@#$%&";

            if (alphabet == "")
            {
                throw new InvalidOperationException("alphabet is empty");
            }

            int length = 16;

            if (atom.ItemCount == 1)
            {
                return new string(Enumerable.Repeat(alphabet, length)
                    .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            }
            else if (atom.ItemCount == 2)
            {
                length = atom.GetAtom(1).GetInt32();

                return new string(Enumerable.Repeat(alphabet, length)
                    .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            }

            throw new InvalidOperationException("rand:string expects zero or one parameter only.");
        });
    }
}
