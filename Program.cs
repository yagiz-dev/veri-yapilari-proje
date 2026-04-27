using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BinTree.DataStructures;

namespace BinTree
{
    class Program
    {
        static void Main()
        {
            BinarySearchTree<int> tree = new BinarySearchTree<int>();

            tree.Insert(5);
            tree.Insert(3);
            tree.Insert(7);
            tree.Insert(8);

            tree.InOrder();

            tree.Insert(9);

            Console.WriteLine();
            
            tree.InOrder();
            
        }
    }
}