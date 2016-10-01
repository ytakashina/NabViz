using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabViz.Encoders
{
    abstract class Encoder
    {
        protected abstract bool[] Encode(object input);
    }
}
