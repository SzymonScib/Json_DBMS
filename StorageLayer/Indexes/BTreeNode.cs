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
            int i = Keys.Count - 1; //i = 2

            if(IsLeaf){  
                Keys.Add(0); 

                while(i >= 0 && key < Keys[i]){ //true
                    Keys[i + 1] = Keys[i]; 
                    i--; 
                }
                Keys[i + 1] = key; //Keys = [5, 6, 10, 12, 20]
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
            y.Keys.RemoveRange(t, t - 1); 

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

        public void Delete(int key){//10
            int idx = FindKey(key);//3
            if(idx < Keys.Count && Keys[idx] == key){//3 < 3 && 10 == 10 false
                if(IsLeaf){
                    RemoveFromLeaf(idx);
                }
                else{
                    RemoveFromNonLeaf(idx);
                }
            }
            else{//true
                if(IsLeaf){//true                
                    if(Keys.Count <= Degree){
                        RemoveFromLeaf(idx);
                        Fill(idx);
                    }
                    else{
                        RemoveFromLeaf(idx);
                    }
                }
                else{
                    if(Children[idx].Keys.Count <= Degree){
                        RemoveFromLeaf(idx);
                        Fill(idx);
                    }
                    if(idx == Keys.Count){
                        Children[idx - 1].Delete(key);
                    }
                    else{
                        Children[idx].Delete(key);
                    }
                }
            }
        }
        
        //Search for an index of a key
        private int FindKey(int key){
            int idx = 0;
            while(idx < Keys.Count && Keys[idx] < key){
                idx++;
            }
            return idx;
        }

        private void RemoveFromLeaf(int idx){
            Keys.RemoveAt(idx);
        }

        private void RemoveFromNonLeaf(int idx){
            int key = Keys[idx];//10

            if(Children[idx].Keys.Count >= Degree){//3 >= 3 true
                int pred = GetPred(idx);//10
                //Keys[idx] = pred;//10
                Children[idx].Delete(pred);//10
            }
            else if(Children[idx + 1].Keys.Count >= Degree){
                int succ = GetSucc(idx);
                Keys[idx] = succ;
                Children[idx + 1].Delete(succ);
            }
            else{
                Merge(idx);
                Children[idx].Delete(key);
            }

        }

        private int GetPred(int idx){//1
            BTreeNode cur = Children[idx];//[8 7 10]
            while(!cur.IsLeaf){
                cur = cur.Children[cur.Keys.Count];
            }
            return cur.Keys[cur.Keys.Count - 1];//[10]
        }

        private int GetSucc(int idx){
            BTreeNode cur = Children[idx + 1];
            while(!cur.IsLeaf){
                cur = cur.Children[0];
            }
            return cur.Keys[0];
        }

        private void Merge(int idx){
            BTreeNode child = Children[idx];
            BTreeNode sibling = Children[idx + 1];

            child.Keys.Add(Keys[idx]);

            child.Keys.AddRange(sibling.Keys);
            child.Children.AddRange(sibling.Children);

            Keys.RemoveAt(idx);
            Children.RemoveAt(idx + 1);
        }

        private void BorrowFromPrev(int idx){
            BTreeNode child = Children[idx];
            BTreeNode sibling = Children[idx - 1];

            child.Keys.Insert(0, Keys[idx - 1]);

            if(!child.IsLeaf){
                child.Children.Insert(0, sibling.Children[sibling.Children.Count]);
                sibling.Children.RemoveAt(sibling.Children.Count);
            }

            Keys[idx - 1] = sibling.Keys[sibling.Keys.Count - 1];
            sibling.Keys.RemoveAt(sibling.Keys.Count - 1);
        }

        private void BorrowFromNext(int idx){
            BTreeNode child = Children[idx];
            BTreeNode sibling = Children[idx + 1];

            child.Keys.Add(Keys[idx]);

            if(!child.IsLeaf){
                child.Children.Add(sibling.Children[0]);
                sibling.Children.RemoveAt(0);
            }

            Keys[idx] = sibling.Keys[0];
            sibling.Keys.RemoveAt(0);
        }

        private void Fill(int idx){
            if(idx != 0 && Children[idx - 1].Keys.Count >= Degree){
                BorrowFromPrev(idx);
            }
            else if(idx != Keys.Count && Children[idx + 1].Keys.Count >= Degree){
                BorrowFromNext(idx);
            }
            else{
                if(idx != Keys.Count){//idx != Keys.Count
                    Merge(idx);
                }
                else{
                    Merge(idx - 1);
                }
            }
        }
    }
}