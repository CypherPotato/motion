using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionWindowsClient;

public class MotionConnection
{
    public string Endpoint { get; set; }
    public string? Authorization { get; set; }

    public MotionConnection(string endpoint, string? authorization)
    {
        Endpoint = endpoint;
        Authorization = authorization;
    }
}
