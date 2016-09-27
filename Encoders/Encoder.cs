using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZetaOne.Encoders
{
    abstract class Encoder
    {
        protected abstract bool[] Encode(object input);
    }
}
