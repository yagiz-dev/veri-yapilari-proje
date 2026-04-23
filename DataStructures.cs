using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinTree
{
    internal class DataStructures
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
            public TreeNode<T> Left { get; set; }
            public TreeNode<T> Right { get; set; }
            public TreeNode(T data)
            {
                this.Data = data;
                Left = null;
                Right = null;
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
            TreeNode<T> Root;

            public bool IsEmpty()
            {
                return Root == null;
            }

            private bool isLeaf(TreeNode<T> node)
            {
                if (node == null) throw new ArgumentNullException("Error: Node is null.");
                return node.Left == null && node.Right == null;
            }

            public void Insert(T data)
            {
                if (IsEmpty())
                {
                    Root = new TreeNode<T>(data);
                }
                else
                {
                    InsertRecursively(Root, data);
                }
            }

            private void InsertRecursively(TreeNode<T> node, T data)
            {
                if (IsEmpty()) return;
                if (node == null || data == null) throw new ArgumentNullException("Error: Node or data is null.");

                if (data.CompareTo(node.Data) < 0)
                {
                    if (node.Left == null)
                    {
                        node.Left = new TreeNode<T>(data);
                    }
                    else
                    {
                        InsertRecursively(node.Left, data);
                    }
                }
                else if (data.CompareTo(node.Data) > 0)
                {
                    if (node.Right == null)
                    {
                        node.Right = new TreeNode<T>(data);
                    }
                    else
                    {
                        InsertRecursively(node.Right, data);
                    }
                }
                else if (data.CompareTo(node.Data) == 0)
                {
                    // Duplicate value, do nothing or handle as needed
                }
            }

            public bool Search(T data)
            {
                return SearchRecursively(Root, data);
            }

            private bool SearchRecursively(TreeNode<T> node, T data)
            {
                if (node == null) return false;
                if (data == null) throw new ArgumentNullException("Error: Data is null.");
                if (data.CompareTo(node.Data) == 0)
                {
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

            public void Remove(T data)
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
                }
            }

            
        }
    }
}
