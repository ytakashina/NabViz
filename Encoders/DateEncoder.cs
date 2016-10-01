using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabViz.Encoders
{
    class DateEncoder : Encoder
    {
        protected override bool[] Encode(object input)
        {
            return new[] {true};
        }
    }
}
