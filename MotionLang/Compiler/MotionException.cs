using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionLang.Compiler;

public class MotionException : Exception
{
    internal TextInterpreterSnapshot snapshot;
    public int Line { get => snapshot.Line; }
    public int Position { get => snapshot.Position; }
    public int Column { get => snapshot.Column; }
    public int Length { get => snapshot.Length; }
    public string LineText { get => snapshot.LineText; }

    internal MotionException(string message, TextInterpreter i) : base(message)
    {
        snapshot = i.TakeSnapshot(1);
    }

    internal MotionException(string message, TextInterpreterSnapshot i, Exception? innerException) : base(message, innerException)
    {
        snapshot = i;
    }
}
