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

        public BTree(int degree){
            Degree = degree;
            Root = new BTreeNode(true, degree);
        }

        public void Insert(int key){
            if (Root.Keys.Count == 2 * Degree - 1) {
                BTreeNode newRoot = new BTreeNode(false, Degree);
                newRoot.Children.Add(Root);
                newRoot.SplitChild(0, Root); 
                Root = newRoot;
            }
            Root.InsertNotFull(key);
        }

        public void Delete(int key){//key = 7
            if (Root.Keys.Count == 0){
                return;
            }
            Root.Delete(key);//
            if (Root.Keys.Count == 0 && Root.Children.Count == 1){
                Root = Root.Children[0];
            }
        }

        public BTreeNode? Search(int key){
            if (Root == null){
                return null;
            }
            return Root.Search(key);
        }
    }
}