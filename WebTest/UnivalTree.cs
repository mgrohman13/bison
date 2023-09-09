using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;

namespace WebTest
{
    public class UnivalTree
    {
        public static void Main2(string[] args)
        {
            TreeNode root = new TreeNode(5);
            root.left = new TreeNode(4);
            root.left.left = new TreeNode(4);
            root.left.right = new TreeNode(4);
            root.right = new TreeNode(3);
            root.right.right = new TreeNode(3);
            root.right.right.right = new TreeNode(2);

            Console.WriteLine(CountUnivalTree(root)); // 5             
        }

        public static int CountUnivalTree(TreeNode root)
        {
            int count = 0;
            if (root != null)
            {
                CountUnivalTree(null, root, out _, ref count);
            }
            return count;
        }
        private static void CountUnivalTree(TreeNode parent, TreeNode current,
                out bool isUnival, ref int count)
        {
            isUnival = true;
            if (current != null)
            {
                CountUnivalTree(current, current.left, out bool leftMatches, ref count);
                isUnival &= leftMatches;
                CountUnivalTree(current, current.right, out bool rightMatches, ref count);
                isUnival &= rightMatches;

                if (isUnival)
                    count++;

                isUnival &= (parent != null && parent.Value == current.Value);
            }
        }

        private static void AddChildren(TreeNode root, int value, int level)
        {
            root.left = new TreeNode(value);
            root.right = new TreeNode(value);

            if (level > 0)
            {
                AddChildren(root.left, value, level - 1);
                AddChildren(root.right, value, level - 1);
            }
        }

        public class TreeNode
        {
            public TreeNode left, right;
            public int Value { get; set; }

            public TreeNode(int value)
            {
                this.Value = value;
            }
        }
    }
}
