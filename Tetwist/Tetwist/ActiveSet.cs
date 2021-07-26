using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Tetwist
{
    class ActiveSet
    {
        List<Block> blocks;
        int waitTime;

        public ActiveSet()
        {
            Init();
        }

        void Init()
        {
            List<Point> blocks = new List<Point>();
            List<Point> temp = new List<Point>();
            waitTime = 0;

            int numBlocks = Game.Random.GaussianCappedInt(Game.BlockValSize, Game.BlockValDev, 1)
                + Game.Random.OEInt(Game.BlockOESize);

            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue;
            int curX = 0, curY = 0;
            for (int i = 0 ; i < numBlocks ; ++i)
            {
                Point np;
                do
                {
                    switch (Game.Random.Next(4))
                    {
                    case 0:
                        --curX;
                        break;
                    case 1:
                        --curY;
                        break;
                    case 2:
                        ++curX;
                        break;
                    case 3:
                        ++curY;
                        break;
                    }
                    np = new Point(curX, curY);
                }
                while (temp.Contains(np));
                temp.Add(np);

                minX = Math.Min(minX, curX);
                maxX = Math.Max(maxX, curX);
                minY = Math.Min(minY, curY);
            }

            int diff = maxX - minX + 1;
            if (diff > Game.Width)
            {
                Init();
                return;
            }
            int adjX = Game.Random.Round(( Game.Width - diff ) / 2.0);
            adjX = adjX - minX;

            int cap = maxX + adjX + 1;
            int[] Ys = new int[cap];

            double avgX = 0, avgY = 0;
            bool gameOver = false;
            foreach (Point p in temp)
            {
                int newX = p.X + adjX;
                int newY = p.Y - minY;

                Ys[newX] = Math.Max(Ys[newX], newY);
                blocks.Add(new Point(newX, newY));

                avgX += newX;
                avgY += newY;
            }

            avgX /= numBlocks;
            avgY /= numBlocks;

            int blockCount = blocks.Count;
            List<Point> randList = new List<Point>(blockCount);
            while (blockCount > 0)
            {
                int index = Game.Random.Next(blockCount);
                randList.Add(blocks[index]);
                blocks.RemoveAt(index);
                blockCount = blocks.Count;
            }
            blocks = randList;

            blocks.Sort(new Comparison<Point>(delegate(Point p1, Point p2)
            {
                double xDif, yDif;
                double dist = ( Math.Sqrt(( xDif = p1.X - avgX ) * xDif + ( yDif = p1.Y - avgY ) * yDif) -
                    Math.Sqrt(( xDif = p2.X - avgX ) * xDif + ( yDif = p2.Y - avgY ) * yDif) );
                if (dist < 0)
                    return 1;
                if (dist > 0)
                    return -1;
                return 0;
            }));

            temp.Clear();
            for (int x = minX + adjX ; x < cap ; ++x)
                temp.Add(new Point(x, Ys[x]));

            Block.NewActiveBlock();

            this.blocks = new List<Block>(blocks.Count);
            foreach (Point p in blocks)
            {
                if (Game.HasBlock(p))
                {
                    Game.RemoveBlock(p);
                    new GameOverBlock(p);
                    gameOver = true;
                }
                else
                {
                    Block newInstance = Block.NewInstance(p);
                    this.blocks.Add(newInstance);
                }
            }

            if (gameOver)
            {
                Game.GameOver();
            }
        }

        public void Fall()
        {
            if (++waitTime > Game.NewBlockWaitTime)
            {
                if (!Block.MoveBlocks(blocks, 0, 1))
                    Game.Solidifying = true;
            }
        }

        public bool Rotate()
        {
            for (int i = blocks.Count ; --i > -1 ; )
                if (TryRotate(blocks[i].Point))
                    return true;
            return false;
            //for (int i = 0; ++i < 4; )
            //    if (TryRotate(i))
            //        break;
        }

        bool TryRotate(Point pivot)
        {
            Dictionary<Block, Point> newPoints = new Dictionary<Block, Point>();
            foreach (Block b in blocks)
            {
                int x = b.Point.X, y = b.Point.Y;
                Rotate(ref x, ref y, pivot.X, pivot.Y);
                newPoints.Add(b, new Point(x, y));
            }

            return Block.MoveBlocks(blocks, newPoints);
        }

        void Rotate(ref int x, ref int y, int pivotX, int pivotY)
        {
            x = x - pivotX;
            y = y - pivotY;

            if (x == 0)
            {
                x = -y;
                y = 0;
            }
            else if (y == 0)
            {
                y = x;
                x = 0;
            }
            else
            {
                int tempInt = x;
                x = -y;
                y = tempInt;
            }

            x += pivotX;
            y += pivotY;
        }

        public bool Drop()
        {
            int scoreAmt = 0;
            while (Block.MoveBlocks(blocks, 0, 1))
            {
                ++scoreAmt;
            }
            Game.Score += Game.Random.Round(scoreAmt * Game.DropScoreMult);
            return scoreAmt > 0;
        }

        public bool Move(int xmod, int ymod)
        {
            return Block.MoveBlocks(blocks, xmod, ymod);
        }

        public void NotifySolidified()
        {
            for (int i = blocks.Count ; --i > -1 ; )
            {
                int index = Game.Random.Next(blocks.Count);
                Block b;
                if (!( ( b = blocks[index] ).Dead ))
                    b.Notify(Notifications.Type._Solidification, null);
                blocks.RemoveAt(index);
            }
        }

        public bool Contains(Block b)
        {
            return blocks.Contains(b);
        }
    }
}
