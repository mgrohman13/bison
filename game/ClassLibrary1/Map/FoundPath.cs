using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ClassLibrary1.Map
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class FoundPath(IEnumerable<Point> path, FoundPath target, double movement)
    {
        public readonly double Movement = movement;
        public readonly IReadOnlyList<Point> Path = path.ToList().AsReadOnly();
        private readonly FoundPath Target = target;

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
