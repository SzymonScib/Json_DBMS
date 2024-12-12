using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorageLayer.Indexes
{
    public class BTreeNode
    {
        public List<int> Keys { get; set; }
        public List<BTreeNode> Children { get; set; }
        public bool IsLeaf { get; set; }
        private int Degree { get; set; }

        public BTreeNode(bool isLeaf, int degree)
        {
            Keys = new List<int>();
            Children = new List<BTreeNode>();
            IsLeaf = isLeaf;
            Degree = degree;
        }

        public void InsertNotFull(int key){
            int i = Keys.Count - 1; 

            if(IsLeaf){  
                Keys.Add(0); 

                while(i >= 0 && key < Keys[i]){ 
                    Keys[i + 1] = Keys[i]; 
                    i--; 
                }
                Keys[i + 1] = key; 
            }
            else{
                while(i >= 0 && key < Keys[i]){
                    i--;                    
                }
                i++;

                if(Children[i].Keys.Count == 2 * Degree - 1){
                    SplitChild(i, Children[i]);
                  
                    if(key > Keys[i]){
                        i++;
                    }
                }
                Children[i].InsertNotFull(key);
            }
        }

        public void SplitChild(int i, BTreeNode y){
            int t = y.Degree; 
            BTreeNode z = new BTreeNode(y.IsLeaf, t); 

            z.Keys.AddRange(y.Keys.GetRange(t,  t - 1)); 

            if(!y.IsLeaf){
                z.Children.AddRange(y.Children.GetRange(t, t));
                y.Children.RemoveRange(t, t);
            }

            Children.Insert(i + 1, z);

            Keys.Insert(i, y.Keys[t - 1]);
        }

        public BTreeNode? Search(int key){
            int i = 0;
            while(i < Keys.Count && key > Keys[i]){
                i++;
            }
            if(i < Keys.Count && key == Keys[i]){
                return this;
            }
            if(IsLeaf){
                return null;
            }
            return Children[i].Search(key);
        }
    }
}