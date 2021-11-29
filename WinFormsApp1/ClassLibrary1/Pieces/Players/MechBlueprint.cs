using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Players
{
    public class MechBlueprint
    {
        public readonly double Vision;
        public readonly IKillable.Values Killable;
        public readonly IReadOnlyCollection<IAttacker.Values> Attacks;
        public readonly IMovable.Values Movable;
        internal MechBlueprint(double vision, IKillable.Values killable, List<IAttacker.Values> attacks, IMovable.Values movable)
        {
            this.Vision = vision;
            this.Killable = killable;
            this.Attacks = attacks;
            this.Movable = movable;
        }
    }
}
