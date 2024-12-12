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

        public void InsertNotFull(int key){//key = 6
            int i = Keys.Count - 1; //i = 3

            if(IsLeaf){  
                Keys.Add(0); //Keys = [5, 6, 10, 20, 0]

                while(i >= 0 && key < Keys[i]){//true i true 
                    Keys[i + 1] = Keys[i]; 
                    i--; 
                }
                Keys[i + 1] = key; //Keys = [2, 5, 6, 10, 20]
            }
            //Insertion into a non leaf node
            else{
                while(i >= 0 && key < Keys[i]){
                    i--;                    
                }
                i++;

                //Check if the child is full
                if(Children[i].Keys.Count == 2 * Degree - 1){
                    SplitChild(i, Children[i]);

                    //After split, middle key goes up and the child is split into two                   
                    if(key > Keys[i]){
                        i++;
                    }
                }
                Children[i].InsertNotFull(key);
            }
        }

        public void SplitChild(int i, BTreeNode y){
            int t = y.Degree; //t = 3
            BTreeNode z = new BTreeNode(y.IsLeaf, t); //z.Degree = 3, z.IsLeaf = true z.Keys = [], z.Children = []

            //Move second half of y's keys to z
            z.Keys.AddRange(y.Keys.GetRange(t,  t - 1)); //z.Keys = [10, 20]
            y.Keys.RemoveRange(t, t - 1); //y.Keys = [2, 5, 6]

            //Move second half of y's children to z if y is not a leaf
            if(!y.IsLeaf){
                z.Children.AddRange(y.Children.GetRange(t, t));
                y.Children.RemoveRange(t, t);
            }

            Children.Insert(i + 1, z);

            //Move the middle key of y to this node
            Keys.Insert(i, y.Keys[t - 1]); //this.Keys = [6]
        }

        public BTreeNode? Search(int key){
            int i = 0;
            // Find the first key greater than or equal to the key
            while(i < Keys.Count && key > Keys[i]){
                i++;
            }
             // If the found key is equal to the key, return this node
            if(i < Keys.Count && key == Keys[i]){
                return this;
            }
            // If the key is not found here and this is a leaf node
            if(IsLeaf){
                return null;
            }
            // Go to the appropriate child
            return Children[i].Search(key);
        }
    }
}