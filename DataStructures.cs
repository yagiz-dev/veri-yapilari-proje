using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinTree
{
    class DataStructures
    {
        public class Node<T>
        {
            private T _data;
            public Node<T> Next { get; set; }
            public Node<T> Prev { get; set; }

            public Node(T data)
            {
                this.Data = data;
                Next = null;
            }
            public T Data
            {
                get { return _data; }
                set { _data = value; }
            }
        }

        public class List<T>
        {
            public Node<T> Head { get; set; }
            public Node<T> Tail { get; set; }

            public List()
            {
                Head = null;
                Tail = null;
            }

            public void AddList(T data)
            {
                Node<T> newNode = new Node<T>(data);
                if (Head == null)
                {
                    Head = newNode;
                    Tail = newNode;
                }
                else
                {
                    Tail.Next = newNode;
                    newNode.Prev = Tail;
                    Tail = newNode;
                }
            }

            public void rmLast()
            {
                if (Tail == null) throw new InvalidOperationException("List is empty.");
                if (object.ReferenceEquals(Head, Tail))
                {
                    Head = null;
                    Tail = null;
                }
                else
                {
                    Tail = Tail.Prev;
                    Tail.Next = null;
                }
            }

            public bool Remove(T data)
            {
                for (Node<T> current = Head; current != null; current = current.Next)
                {
                    // Jenerik tiplerin verilerini güvenli şekilde karşılaştırır
                    if (EqualityComparer<T>.Default.Equals(current.Data, data))
                    {
                        if (object.ReferenceEquals(current, Head))
                        {
                            Head = current.Next;
                            if (Head != null)
                            {
                                Head.Prev = null;
                            }
                            else
                            {
                                Tail = null; // Liste boşaldı
                            }
                        }
                        else if (object.ReferenceEquals(current, Tail))
                        {
                            Tail = current.Prev;
                            if (Tail != null)
                            {
                                Tail.Next = null;
                            }
                            else
                            {
                                Head = null; // Liste boşaldı
                            }
                        }
                        else
                        {
                            current.Prev.Next = current.Next;
                            current.Next.Prev = current.Prev;
                        }

                        return true; // İlk bulunan veri başarıyla silindi
                    }
                }

                return false; // Veri listede bulunamadı
            }

            public void PrintList()
            {
                if (Head == null) throw new InvalidOperationException("List is empty.");
                for (Node<T> current = Head; current != null; current = current.Next)
                {
                    Console.WriteLine(current.Data);
                }
            }
        }

        public class Queue<T>
        {
            Node<T> head;
            Node<T> tail;

            public void Enqueue(T data)
            {
                Node<T> newNode = new Node<T>(data);
                if (tail == null)
                {
                    head = newNode;
                    tail = newNode;
                }
                else
                {
                    tail.Next = newNode;
                    tail = newNode;
                }
            }

            public T Dequeue()
            {
                if (head == null) throw new InvalidOperationException("Queue is empty.");
                T data = head.Data;
                head = head.Next;
                if (head == null) { tail = null; } // Queue is now empty 
                return data;
            }

            public void Peek()
            {
                if (head == null) throw new InvalidOperationException("Queue is empty.");
                Console.WriteLine(head.Data);
            }

            public bool IsEmpty()
            {
                return head == null;
            }

        }

        public class TreeNode<T>
        {
            private T _data;
            private int _height = 0;
            public TreeNode<T> Left { get; set; }
            public TreeNode<T> Right { get; set; }

            public TreeNode(T data)
            {
                this.Data = data;
                Left = null;
                Right = null;
            }

            public int Height
            {
                get { return _height; }
                set { _height = value; }
            }


            public T Data
            {
                get { return _data; }
                set { _data = value; }
            }
        }

        public class BinarySearchTree<T> where T : IComparable<T>
        //Ağacınız int, string, double veya DateTime gibi C#'ın yerleşik yapılarıyla kusursuz çalışır, çünkü bu yapıların hepsi Microsoft tarafından IComparable olarak tasarlanmıştır.
        {
            TreeNode<T> Root = null;

            public bool IsEmpty()
            {
                return Root == null;
            }

            private bool isLeaf(TreeNode<T> node)
            {
                if (node == null) throw new ArgumentNullException("Error: Node is null.");
                return node.Left == null && node.Right == null;
            }

            private int GetHeight(TreeNode<T> node)
            {
                return node == null ? 0 : node.Height;
            }

            private int GetBalanceFactor(TreeNode<T> node)
            {
                if (node == null) return 0;
                return GetHeight(node.Left) - GetHeight(node.Right);
            }

            public void Insert(T data)
            {
                if (IsEmpty())
                {                    
                    Root = new TreeNode<T>(data);
                }
                else
                {
                    Root = InsertRecursively(Root, data);
                }
            }
 
            private void CheckBalance(TreeNode<T> node)
            {
                if (node == null) return;            
                if (GetBalanceFactor(node) > 1)
                {
                    if (GetHeight(node.Left) > GetHeight(node.Right))
                    {
                        if(GetHeight(node.Left.Left) >= GetHeight(node.Left.Right))
                        {
                            // Sağ Rotasyon
                            node = RotateRight(node);
                        }
                        else
                        {
                            // Sol-Sağ Rotasyon
                            node.Left = RotateLeft(node.Left);
                            node = RotateRight(node);
                        }
                    }
                    else
                    {
                        if(GetHeight(node.Right.Right) >= GetHeight(node.Right.Left))
                        {
                            // Sol Rotasyon
                            node = RotateLeft(node);
                        }
                        else
                        {
                            // Sağ-Sol Rotasyon
                            node.Right = RotateRight(node.Right);
                            node = RotateLeft(node);
                        }
                    }
                }
                return;
            }
            
            private TreeNode<T> RotateRight(TreeNode<T> node)
            {
                TreeNode<T> newRoot = node.Left;
                node.Left = newRoot.Right;
                newRoot.Right = node;
                node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;
                newRoot.Height = Math.Max(GetHeight(newRoot.Left), GetHeight(newRoot.Right)) + 1;                
                return newRoot;
            }

            private TreeNode<T> RotateLeft(TreeNode<T> node)
            {
                TreeNode<T> newRoot = node.Right;
                node.Right = newRoot.Left;
                newRoot.Left = node;
                node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;
                newRoot.Height = Math.Max(GetHeight(newRoot.Left), GetHeight(newRoot.Right)) + 1;
                return newRoot;
            }

            private TreeNode<T> InsertRecursively(TreeNode<T> node, T data)
            {
                if (node == null) 
                {
                    TreeNode<T> newNode = new TreeNode<T>(data);
                    newNode.Height = 1; 
                    return newNode;
                }

                if (data.CompareTo(node.Data) < 0)
                {
                    node.Left = InsertRecursively(node.Left, data);
                }
                else if (data.CompareTo(node.Data) > 0)
                {
                    node.Right = InsertRecursively(node.Right, data);
                }
                else
                {
                    return node;
                }

                node.Height = Math.Max(GetHeight(node.Left), GetHeight(node.Right)) + 1;

                int balance = GetBalanceFactor(node);

                if (balance > 1) 
                {
                    if (data.CompareTo(node.Left.Data) < 0) 
                        return RotateRight(node); 
                    
                    if (data.CompareTo(node.Left.Data) > 0) 
                    {
                        node.Left = RotateLeft(node.Left);
                        return RotateRight(node);
                    }
                }
                if (balance < -1)
                {
                    if (data.CompareTo(node.Right.Data) > 0)
                        return RotateLeft(node);

                    if (data.CompareTo(node.Right.Data) < 0)
                    {
                        node.Right = RotateRight(node.Right);
                        return RotateLeft(node);
                    }
                }
                return node; 
            }

            public bool Search(T data)
            {
                return SearchRecursively(Root, data);
            }

            private bool SearchRecursively(TreeNode<T> node, T data)
            {
                if (node == null || data == null) { Console.WriteLine("Error: \"" + data + "\" not found."); throw new ArgumentNullException("Error: Node or data is null."); }

                if (data.CompareTo(node.Data) == 0)
                {
                    Console.WriteLine("Data found! Height: {0}", node.Height);
                    return true;
                }
                else if (data.CompareTo(node.Data) < 0)
                {
                    return SearchRecursively(node.Left, data);
                }
                else
                {
                    return SearchRecursively(node.Right, data);
                }
            }

            public void Remove(T data)//silinecek düğümün sol çocuuğunun en sağ çocuğunu veya sağ çocuğunun en sol çocuğu ile değiştirilmesi işlemi yapılır
            {
                if (Root == null) throw new InvalidOperationException("Tree is empty.");
                if (data == null) throw new ArgumentNullException(nameof(data));

                TreeNode<T> parent = null;
                TreeNode<T> current = Root;

                while (current != null && current.Data.CompareTo(data) != 0)
                {
                    parent = current;
                    if (data.CompareTo(current.Data) < 0)
                        current = current.Left;
                    else
                        current = current.Right;
                }

                if (current == null)
                {
                    Console.WriteLine("Data not found in the tree.");
                    return;
                }

                if (current.Left != null && current.Right != null)
                {
                    TreeNode<T> successorParent = current;
                    TreeNode<T> successor = current.Right;

                    while (successor.Left != null)
                    {
                        successorParent = successor;
                        successor = successor.Left;
                    }

                    current.Data = successor.Data;
                    current = successor;
                    parent = successorParent;
                }

                TreeNode<T> child = current.Left ?? current.Right;

                if (parent == null)
                {
                    Root = child;
                }
                else if (parent.Left == current)
                {
                    parent.Left = child;
                }
                else
                {
                    parent.Right = child;
                }
            }
           
            public void PreOrder()
            {
                // ==========================================
                // PRE-ORDER (KÖK - SOL - SAĞ)
                // ==========================================

                Console.Write("Pre-Order  : ");
                PreOrderRecursive(Root);
                Console.WriteLine();
            }

            private void PreOrderRecursive(TreeNode<T> node)
            {
                if (node != null)
                {
                    Console.Write(node.Data + " "); // Önce Kök
                    Console.WriteLine("Level: " + node.Height); // Düğümün seviyesini yazdır
                    PreOrderRecursive(node.Left);   // Sonra Sol
                    PreOrderRecursive(node.Right);  // En Son Sağ
                }
            }

            public void InOrder()
            {
                // ==========================================
                // IN-ORDER (SOL - KÖK - SAĞ)
                // ==========================================

                Console.Write("In-Order   : ");
                InOrderRecursive(Root);
                Console.WriteLine();
            }

            private void InOrderRecursive(TreeNode<T> node)
            {
                if (node != null)
                {
                    InOrderRecursive(node.Left);    // Önce Sol
                    Console.Write(node.Data + " "); // Sonra Kök
                    Console.WriteLine("Level: " + node.Height); // Düğümün seviyesini yazdır
                    InOrderRecursive(node.Right);   // En Son Sağ
                }
            }

            public void PostOrder()
            {
                // ==========================================
                // POST-ORDER (SOL - SAĞ - KÖK)
                // ==========================================

                Console.Write("Post-Order : ");
                PostOrderRecursive(Root);
                Console.WriteLine();
            }

            private void PostOrderRecursive(TreeNode<T> node)
            {
                if (node != null)
                {
                    PostOrderRecursive(node.Left);  // Önce Sol
                    PostOrderRecursive(node.Right); // Sonra Sağ
                    Console.Write(node.Data + " "); // En Son Kök
                    Console.WriteLine("Level: " + node.Height); // Düğümün seviyesini yazdır
                }
            }

            
        }
    }
}
