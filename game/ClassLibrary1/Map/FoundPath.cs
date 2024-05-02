using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1.Map
{
    [Serializable]
    public class FoundPath
    {
        public readonly double Movement;
        public readonly IReadOnlyList<Point> Path;
        private readonly FoundPath Target;

        public FoundPath(IEnumerable<Point> path, FoundPath target, double movement)
        {
            this.Movement = movement;
            this.Path = path.ToList().AsReadOnly();
            this.Target = target;
        }

        internal IEnumerable<Point> CompletePath(Point start)
        {
            bool flag = false;
            for (int a = 0; a < Path.Count; a++)
            {
                Point point = Path[a];
                if (flag)
                    yield return point;
                else
                    flag = start == point;
            }
            if (!flag)
                throw new Exception();

            if (Target != null)
                foreach (var p in Target.CompletePath(Path[^1]))
                    yield return p;
        }
    }
}
