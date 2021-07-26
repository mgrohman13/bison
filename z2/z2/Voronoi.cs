using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public static class Voronoi
    {
        public static VoronoiGraph GetVoronoiGraph(IEnumerable<Map.HeightPoint> points, double minX, double maxX, double minY, double maxY)
        {
            BinaryPriorityQueue<VEvent> PQ = new BinaryPriorityQueue<VEvent>();
            Dictionary<VDataNode, VCircleEvent> CurrentCircles = new Dictionary<VDataNode, VCircleEvent>();
            VoronoiGraph VG = new VoronoiGraph();
            VNode RootNode = null;
            foreach (Map.HeightPoint V in points)
            {
                PQ.Push(new VDataEvent(new VoronoiVertex(V.Point.X, V.Point.Y)));
            }
            while (PQ.Count > 0)
            {
                VEvent VE = PQ.Pop() as VEvent;
                VDataNode[] CircleCheckList;
                if (VE is VDataEvent)
                {
                    RootNode = VNode.ProcessDataEvent(VE as VDataEvent, RootNode, VG, VE.Y, out CircleCheckList);
                }
                else if (VE is VCircleEvent)
                {
                    CurrentCircles.Remove(( (VCircleEvent)VE ).NodeN);
                    if (!( (VCircleEvent)VE ).Valid)
                        continue;
                    RootNode = VNode.ProcessCircleEvent(VE as VCircleEvent, RootNode, VG, VE.Y, out CircleCheckList);
                }
                else
                    throw new Exception("Got event of type " + VE.GetType().ToString() + "!");
                foreach (VDataNode VD in CircleCheckList)
                {
                    if (CurrentCircles.ContainsKey(VD))
                    {
                        ( (VCircleEvent)CurrentCircles[VD] ).Valid = false;
                        CurrentCircles.Remove(VD);
                    }
                    VCircleEvent VCE = VNode.CircleCheckDataNode(VD, VE.Y);
                    if (VCE != null)
                    {
                        PQ.Push(VCE);
                        CurrentCircles[VD] = VCE;
                    }
                }
                if (VE is VDataEvent)
                {
                    VoronoiVertex DP = ( (VDataEvent)VE ).DataPoint;
                    foreach (VCircleEvent VCE in CurrentCircles.Values)
                    {
                        if (Dist(DP.Point.X, DP.Point.Y, VCE.Center.Point.X, VCE.Center.Point.Y) < VCE.Y - VCE.Center.Point.Y &&
                            Math.Abs(Dist(DP.Point.X, DP.Point.Y, VCE.Center.Point.X, VCE.Center.Point.Y) - ( VCE.Y - VCE.Center.Point.Y )) > 1e-10)
                            VCE.Valid = false;
                    }
                }
            }
            foreach (VoronoiEdge edge in VG.Edges)
            {
                edge.VertexA = TrimGraph(edge.VertexA, minX, maxX, minY, maxY);
                edge.VertexB = TrimGraph(edge.VertexB, minX, maxX, minY, maxY);
            }
            return VG;
        }

        private static VoronoiVertex TrimGraph(VoronoiVertex voronoiVertex, double minX, double maxX, double minY, double maxY)
        {
            if (voronoiVertex != null)
            {
                PointD p = voronoiVertex.Point;
                if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
                    return null;
            }
            return voronoiVertex;
        }
        private static double ParabolicCut(double x1, double y1, double x2, double y2, double ys)
        {
            if (Math.Abs(x1 - x2) < 1e-10 && Math.Abs(y1 - y2) < 1e-10)
            {
                throw new Exception("Identical datapoints are not allowed!");
            }
            if (Math.Abs(y1 - ys) < 1e-10 && Math.Abs(y2 - ys) < 1e-10)
                return ( x1 + x2 ) / 2;
            if (Math.Abs(y1 - ys) < 1e-10)
                return x1;
            if (Math.Abs(y2 - ys) < 1e-10)
                return x2;
            double a1 = 1 / ( 2 * ( y1 - ys ) );
            double a2 = 1 / ( 2 * ( y2 - ys ) );
            if (Math.Abs(a1 - a2) < 1e-10)
                return ( x1 + x2 ) / 2;
            double xs1 = 0.5 / ( 2 * a1 - 2 * a2 ) * ( 4 * a1 * x1 - 4 * a2 * x2 + 2 *
                Math.Sqrt(-8 * a1 * x1 * a2 * x2 - 2 * a1 * y1 + 2 * a1 * y2 + 4 * a1 * a2 * x2 * x2 + 2 * a2 * y1 + 4 * a2 * a1 * x1 * x1 - 2 * a2 * y2) );
            double xs2 = 0.5 / ( 2 * a1 - 2 * a2 ) * ( 4 * a1 * x1 - 4 * a2 * x2 - 2 *
                Math.Sqrt(-8 * a1 * x1 * a2 * x2 - 2 * a1 * y1 + 2 * a1 * y2 + 4 * a1 * a2 * x2 * x2 + 2 * a2 * y1 + 4 * a2 * a1 * x1 * x1 - 2 * a2 * y2) );
            xs1 = Math.Round(xs1, 10);
            xs2 = Math.Round(xs2, 10);
            if (xs1 > xs2)
            {
                double h = xs1;
                xs1 = xs2;
                xs2 = h;
            }
            if (y1 >= y2)
                return xs2;
            return xs1;
        }
        private static VoronoiVertex CircumCircleCenter(VoronoiVertex A, VoronoiVertex B, VoronoiVertex C)
        {
            if (A == B || B == C || A == C)
                throw new Exception("Need three different points!");
            double tx = ( A.Point.X + C.Point.X ) / 2;
            double ty = ( A.Point.Y + C.Point.Y ) / 2;
            double vx = ( B.Point.X + C.Point.X ) / 2;
            double vy = ( B.Point.Y + C.Point.Y ) / 2;
            double ux, uy, wx, wy;
            if (A.Point.X == C.Point.X)
            {
                ux = 1;
                uy = 0;
            }
            else
            {
                ux = ( C.Point.Y - A.Point.Y ) / ( A.Point.X - C.Point.X );
                uy = 1;
            }
            if (B.Point.X == C.Point.X)
            {
                wx = -1;
                wy = 0;
            }
            else
            {
                wx = ( B.Point.Y - C.Point.Y ) / ( B.Point.X - C.Point.X );
                wy = -1;
            }
            double alpha = ( wy * ( vx - tx ) - wx * ( vy - ty ) ) / ( ux * wy - wx * uy );
            return new VoronoiVertex(tx + alpha * ux, ty + alpha * uy);
        }
        private static double Dist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(( x2 - x1 ) * ( x2 - x1 ) + ( y2 - y1 ) * ( y2 - y1 ));
        }
        private static int ccw(double P0x, double P0y, double P1x, double P1y, double P2x, double P2y, bool PlusOneOnZeroDegrees)
        {
            double dx1, dx2, dy1, dy2;
            dx1 = P1x - P0x;
            dy1 = P1y - P0y;
            dx2 = P2x - P0x;
            dy2 = P2y - P0y;
            if (dx1 * dy2 > dy1 * dx2)
                return +1;
            if (dx1 * dy2 < dy1 * dx2)
                return -1;
            if (( dx1 * dx2 < 0 ) || ( dy1 * dy2 < 0 ))
                return -1;
            if (( dx1 * dx1 + dy1 * dy1 ) < ( dx2 * dx2 + dy2 * dy2 ) && PlusOneOnZeroDegrees)
                return +1;
            return 0;
        }

        private abstract class VNode
        {
            private VNode _Parent = null;
            private VNode _Left = null, _Right = null;
            private VNode Left
            {
                get
                {
                    return _Left;
                }
                set
                {
                    _Left = value;
                    value.Parent = this;
                }
            }
            private VNode Right
            {
                get
                {
                    return _Right;
                }
                set
                {
                    _Right = value;
                    value.Parent = this;
                }
            }
            private VNode Parent
            {
                get
                {
                    return _Parent;
                }
                set
                {
                    _Parent = value;
                }
            }
            private void Replace(VNode ChildOld, VNode ChildNew)
            {
                if (Left == ChildOld)
                    Left = ChildNew;
                else if (Right == ChildOld)
                    Right = ChildNew;
                else
                    throw new Exception("Child not found!");
                ChildOld.Parent = null;
            }
            private static VDataNode LeftDataNode(VDataNode Current)
            {
                VNode C = Current;
                //1. Up
                do
                {
                    if (C.Parent == null)
                        return null;
                    if (C.Parent.Left == C)
                    {
                        C = C.Parent;
                        continue;
                    }
                    else
                    {
                        C = C.Parent;
                        break;
                    }
                } while (true);
                //2. One Left
                C = C.Left;
                //3. Down
                while (C.Right != null)
                    C = C.Right;
                return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
            }
            private static VDataNode RightDataNode(VDataNode Current)
            {
                VNode C = Current;
                //1. Up
                do
                {
                    if (C.Parent == null)
                        return null;
                    if (C.Parent.Right == C)
                    {
                        C = C.Parent;
                        continue;
                    }
                    else
                    {
                        C = C.Parent;
                        break;
                    }
                } while (true);
                //2. One Right
                C = C.Right;
                //3. Down
                while (C.Left != null)
                    C = C.Left;
                return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
            }
            private static VEdgeNode EdgeToRightDataNode(VDataNode Current)
            {
                VNode C = Current;
                //1. Up
                do
                {
                    if (C.Parent == null)
                        throw new Exception("No Left Leaf found!");
                    if (C.Parent.Right == C)
                    {
                        C = C.Parent;
                        continue;
                    }
                    else
                    {
                        C = C.Parent;
                        break;
                    }
                } while (true);
                return (VEdgeNode)C;
            }
            private static VDataNode FindDataNode(VNode Root, double ys, double x)
            {
                VNode C = Root;
                do
                {
                    if (C is VDataNode)
                        return (VDataNode)C;
                    if (( (VEdgeNode)C ).Cut(ys, x) < 0)
                        C = C.Left;
                    else
                        C = C.Right;
                } while (true);
            }
            // Will return the new root (unchanged except in start-up)
            public static VNode ProcessDataEvent(VDataEvent e, VNode Root, VoronoiGraph VG, double ys, out VDataNode[] CircleCheckList)
            {
                if (Root == null)
                {
                    Root = new VDataNode(e.DataPoint);
                    CircleCheckList = new VDataNode[] { (VDataNode)Root };
                    return Root;
                }
                //1. Find the node to be replaced
                VNode C = VNode.FindDataNode(Root, ys, e.DataPoint.Point.X);
                //2. Create the subtree (ONE Edge, but two VEdgeNodes)
                VoronoiEdge VE = new VoronoiEdge();
                VE.LeftPolygon = ( (VDataNode)C ).DataPoint;
                VE.RightPolygon = e.DataPoint;
                VE.VertexA = null;
                VE.VertexB = null;
                VG.Edges.Add(VE);
                VNode SubRoot;
                if (Math.Abs(VE.LeftPolygon.Point.Y - VE.RightPolygon.Point.Y) < 1e-10)
                {
                    if (VE.LeftPolygon.Point.X < VE.RightPolygon.Point.X)
                    {
                        SubRoot = new VEdgeNode(VE, false);
                        SubRoot.Left = new VDataNode(VE.LeftPolygon);
                        SubRoot.Right = new VDataNode(VE.RightPolygon);
                    }
                    else
                    {
                        SubRoot = new VEdgeNode(VE, true);
                        SubRoot.Left = new VDataNode(VE.RightPolygon);
                        SubRoot.Right = new VDataNode(VE.LeftPolygon);
                    }
                    CircleCheckList = new VDataNode[] { (VDataNode)SubRoot.Left, (VDataNode)SubRoot.Right };
                }
                else
                {
                    SubRoot = new VEdgeNode(VE, false);
                    SubRoot.Left = new VDataNode(VE.LeftPolygon);
                    SubRoot.Right = new VEdgeNode(VE, true);
                    SubRoot.Right.Left = new VDataNode(VE.RightPolygon);
                    SubRoot.Right.Right = new VDataNode(VE.LeftPolygon);
                    CircleCheckList = new VDataNode[] { (VDataNode)SubRoot.Left, (VDataNode)SubRoot.Right.Left, (VDataNode)SubRoot.Right.Right };
                }
                //3. Apply subtree
                if (C.Parent == null)
                    return SubRoot;
                C.Parent.Replace(C, SubRoot);
                return Root;
            }
            public static VNode ProcessCircleEvent(VCircleEvent e, VNode Root, VoronoiGraph VG, double ys, out VDataNode[] CircleCheckList)
            {
                VDataNode a, b, c;
                VEdgeNode eu, eo;
                b = e.NodeN;
                a = VNode.LeftDataNode(b);
                c = VNode.RightDataNode(b);
                if (a == null || b.Parent == null || c == null || !a.DataPoint.Equals(e.NodeL.DataPoint) || !c.DataPoint.Equals(e.NodeR.DataPoint))
                {
                    CircleCheckList = new VDataNode[] { };
                    return Root; // Abbruch da sich der Graph ver?ndert hat
                }
                eu = (VEdgeNode)b.Parent;
                CircleCheckList = new VDataNode[] { a, c };
                //1. Create the new Vertex
                VoronoiVertex VNew = new VoronoiVertex(e.Center.Point.X, e.Center.Point.Y);
                //VG.Vertices.Add(VNew);
                //2. Find out if a or c are in a distand part of the tree (the other is then b's sibling) and assign the new vertex
                if (eu.Left == b) // c is sibling
                {
                    eo = VNode.EdgeToRightDataNode(a);
                    // replace eu by eu's Right
                    eu.Parent.Replace(eu, eu.Right);
                }
                else // a is sibling
                {
                    eo = VNode.EdgeToRightDataNode(b);
                    // replace eu by eu's Left
                    eu.Parent.Replace(eu, eu.Left);
                }
                eu.Edge.AddVertex(VNew);
                //          ///////////////////// uncertain
                //          if(eo==eu)
                //              return Root;
                //          /////////////////////
                eo.Edge.AddVertex(VNew);
                //2. Replace eo by new Edge
                VoronoiEdge VE = new VoronoiEdge();
                VE.LeftPolygon = a.DataPoint;
                VE.RightPolygon = c.DataPoint;
                VE.AddVertex(VNew);
                VG.Edges.Add(VE);
                VEdgeNode VEN = new VEdgeNode(VE, false);
                VEN.Left = eo.Left;
                VEN.Right = eo.Right;
                if (eo.Parent == null)
                    return VEN;
                eo.Parent.Replace(eo, VEN);
                return Root;
            }
            public static VCircleEvent CircleCheckDataNode(VDataNode n, double ys)
            {
                VDataNode l = VNode.LeftDataNode(n);
                VDataNode r = VNode.RightDataNode(n);
                if (l == null || r == null || l.DataPoint == r.DataPoint || l.DataPoint == n.DataPoint || n.DataPoint == r.DataPoint)
                    return null;
                if (ccw(l.DataPoint.Point.X, l.DataPoint.Point.Y, n.DataPoint.Point.X, n.DataPoint.Point.Y,
                    r.DataPoint.Point.X, r.DataPoint.Point.Y, false) <= 0)
                    return null;
                VoronoiVertex Center = Voronoi.CircumCircleCenter(l.DataPoint, n.DataPoint, r.DataPoint);
                VCircleEvent VC = new VCircleEvent();
                VC.NodeN = n;
                VC.NodeL = l;
                VC.NodeR = r;
                VC.Center = Center;
                VC.Valid = true;
                if (VC.Y >= ys)
                    return VC;
                return null;
            }
        }

        private class VDataNode : VNode
        {
            public VDataNode(VoronoiVertex DP)
            {
                this.DataPoint = DP;
            }
            public VoronoiVertex DataPoint;
        }

        private class VEdgeNode : VNode
        {
            public VEdgeNode(VoronoiEdge E, bool Flipped)
            {
                this.Edge = E;
                this.Flipped = Flipped;
            }
            public VoronoiEdge Edge;
            private bool Flipped;
            public double Cut(double ys, double x)
            {
                if (!Flipped)
                    return Math.Round(x - Voronoi.ParabolicCut(Edge.LeftPolygon.Point.X, Edge.LeftPolygon.Point.Y,
                        Edge.RightPolygon.Point.X, Edge.RightPolygon.Point.Y, ys), 10);
                return Math.Round(x - Voronoi.ParabolicCut(Edge.RightPolygon.Point.X, Edge.RightPolygon.Point.Y,
                    Edge.LeftPolygon.Point.X, Edge.LeftPolygon.Point.Y, ys), 10);
            }
        }

        private abstract class VEvent : IComparable
        {
            public abstract double Y
            {
                get;
            }
            public abstract double X
            {
                get;
            }
            #region IComparable Members
            public int CompareTo(object obj)
            {
                if (!( obj is VEvent ))
                    throw new ArgumentException("obj not VEvent!");
                int i = Y.CompareTo(( (VEvent)obj ).Y);
                if (i != 0)
                    return i;
                return X.CompareTo(( (VEvent)obj ).X);
            }
            #endregion
        }

        private class VDataEvent : VEvent
        {
            public VoronoiVertex DataPoint;
            public VDataEvent(VoronoiVertex DP)
            {
                this.DataPoint = DP;
            }
            public override double Y
            {
                get
                {
                    return DataPoint.Point.Y;
                }
            }
            public override double X
            {
                get
                {
                    return DataPoint.Point.X;
                }
            }
        }

        private class VCircleEvent : VEvent
        {
            public VDataNode NodeN, NodeL, NodeR;
            public VoronoiVertex Center;
            public override double Y
            {
                get
                {
                    return Math.Round(Center.Point.Y + Dist(NodeN.DataPoint.Point.X, NodeN.DataPoint.Point.Y, Center.Point.X, Center.Point.Y), 10);
                }
            }
            public override double X
            {
                get
                {
                    return Center.Point.X;
                }
            }
            public bool Valid = true;
        }

        private class BinaryPriorityQueue<T>
        {
            private List<T> InnerList = new List<T>();
            private IComparer<T> Comparer;

            public BinaryPriorityQueue()
                : this(System.Collections.Generic.Comparer<T>.Default)
            {
            }
            private BinaryPriorityQueue(IComparer<T> c)
            {
                Comparer = c;
            }

            private void SwitchElements(int i, int j)
            {
                T h = InnerList[i];
                InnerList[i] = InnerList[j];
                InnerList[j] = h;
            }
            private int OnCompare(int i, int j)
            {
                return Comparer.Compare(InnerList[i], InnerList[j]);
            }

            public int Push(T O)
            {
                int p = InnerList.Count, p2;
                InnerList.Add(O);
                do
                {
                    if (p == 0)
                        break;
                    p2 = ( p - 1 ) / 2;
                    if (OnCompare(p, p2) < 0)
                    {
                        SwitchElements(p, p2);
                        p = p2;
                    }
                    else
                        break;
                } while (true);
                return p;
            }
            public T Pop()
            {
                T result = InnerList[0];
                int p = 0, p1, p2, pn;
                InnerList[0] = InnerList[InnerList.Count - 1];
                InnerList.RemoveAt(InnerList.Count - 1);
                do
                {
                    pn = p;
                    p1 = 2 * p + 1;
                    p2 = 2 * p + 2;
                    if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
                        p = p1;
                    if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
                        p = p2;
                    if (p == pn)
                        break;
                    SwitchElements(p, pn);
                } while (true);
                return result;
            }

            public int Count
            {
                get
                {
                    return InnerList.Count;
                }
            }
        }
    }

    public class VoronoiGraph
    {
        //public readonly HashSet<VoronoiVertex> Vertices = new HashSet<VoronoiVertex>();
        public readonly HashSet<VoronoiEdge> Edges = new HashSet<VoronoiEdge>();
    }

    public class VoronoiEdge
    {
        public VoronoiVertex VertexA = null, VertexB = null;

        public VoronoiVertex RightPolygon, LeftPolygon;

        public void AddVertex(VoronoiVertex V)
        {
            if (VertexA == null)
                VertexA = V;
            else if (VertexB == null)
                VertexB = V;
            else
                throw new Exception("Tried to add third vertex!");
        }
    }

    public class VoronoiVertex
    {
        public readonly PointD Point;

        public VoronoiVertex(double x, double y)
        {
            Point = new PointD(x, y);
        }

        public override bool Equals(object obj)
        {
            return Point.Equals(( (VoronoiVertex)obj ).Point);
        }
        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
        public override string ToString()
        {
            return Point.ToString();
        }
    }
}
