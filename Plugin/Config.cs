using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace NonSnappingLimbs
{
    public class Config
    {
        [ModOption(name: "Die on Head Chop", tooltip: "Should enemies die when they are decapitated?", defaultValueIndex = 1)]
        public static bool dieOnHeadChop = false;
        [ModOption(name: "Destabilize One Leg", tooltip: "Should enemies missing a leg fall down?", defaultValueIndex = 1)]
        public static bool destabilizeOneLeg = true;
    }
}
