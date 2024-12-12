using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorageLayer.Indexes
{
    public class BTree
    {
        public BTreeNode Root {get;set;}
        private int Degree {get;set;}

        public BTree(int degree)
        {
            Degree = degree;
            Root = new BTreeNode(true, degree);
        }

        public void Insert(int key){
            if (Root.Keys.Count == 2 * Degree - 1) // 5 = 5 true
            {
                BTreeNode newRoot = new BTreeNode(false, Degree);
                newRoot.Children.Add(Root);
                newRoot.SplitChild(0, Root); // root = [5, 6, 10, 12, 20]
                Root = newRoot;
            }
            Root.InsertNotFull(key);//key = 12
        }

        public BTreeNode? Search(int key){
            if (Root == null)
            {
                return null;
            }
            return Root.Search(key);
        }
    }
}