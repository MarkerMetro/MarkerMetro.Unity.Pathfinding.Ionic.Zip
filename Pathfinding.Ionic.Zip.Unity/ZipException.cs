using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Pathfinding.Ionic.Zip
{
    [Guid("ebc25cf6-9120-4283-b972-0e5520d00006")]
    public class ZipException : Exception
    {
        public ZipException()
        {
        }

        public ZipException(string message)
            : base(message)
        {
            
        }

        public ZipException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
