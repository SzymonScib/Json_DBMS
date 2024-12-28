using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public void Delete(int key){
            if (Root.Keys.Count == 0){
                return;
            }
            Root.Delete(key);
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

        public List<int> RangeQuery(int min, int max){
            List<int> result = new List<int>();
            RangeQueyHelper(Root, min, max, result);
            return result;
        }

        private void RangeQueyHelper(BTreeNode node, int min, int max, List<int> result){
            int i = 0;
            while (i < node.Keys.Count && node.Keys[i] < min){
                i++;
            }
            if (i < node.Keys.Count){
                while (i < node.Keys.Count && node.Keys[i] <= max){
                    if (node.IsLeaf){
                        result.Add(node.Keys[i]);
                    }
                    else{
                        RangeQueyHelper(node.Children[i], min, max, result);
                    }
                    i++;
                }
                if (!node.IsLeaf){
                    RangeQueyHelper(node.Children[i], min, max, result);
                }
            }
            else{
                if (!node.IsLeaf){
                    RangeQueyHelper(node.Children[i], min, max, result);
                }
            }
        }

        public string Serialize(){
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static BTree Deserialize(string json){
            return JsonConvert.DeserializeObject<BTree>(json);
        }
    }
}