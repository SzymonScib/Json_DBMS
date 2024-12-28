using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorageLayer.Indexes;

namespace SZBD.Tests
{
    public class BTreeTests{
        [Fact]
        public void TestInsert(){
            BTree btree = CreateBTree(3);

            Assert.Equal(6, btree.Root.Keys[0]);
            Assert.Equal(10, btree.Root.Keys[1]);
            Assert.Equal(14, btree.Root.Keys[2]);
            Assert.Equal(19, btree.Root.Keys[3]);
            Assert.Equal(1, btree.Root.Children[0].Keys[0]);
            Assert.Equal(2, btree.Root.Children[0].Keys[1]);
            Assert.Equal(5, btree.Root.Children[0].Keys[2]);
            Assert.Equal(6, btree.Root.Children[0].Keys[3]);
            Assert.Equal(7, btree.Root.Children[1].Keys[0]);
            Assert.Equal(8, btree.Root.Children[1].Keys[1]);
            Assert.Equal(10, btree.Root.Children[1].Keys[2]);
            Assert.Equal(11, btree.Root.Children[2].Keys[0]);
            Assert.Equal(12, btree.Root.Children[2].Keys[1]);
            Assert.Equal(13, btree.Root.Children[2].Keys[2]);
            Assert.Equal(14, btree.Root.Children[2].Keys[3]);
            Assert.Equal(17, btree.Root.Children[3].Keys[0]);
            Assert.Equal(19, btree.Root.Children[3].Keys[1]);
            Assert.Equal(20, btree.Root.Children[4].Keys[0]);
            Assert.Equal(25, btree.Root.Children[4].Keys[1]);
            Assert.Equal(28, btree.Root.Children[4].Keys[2]);
            Assert.Equal(30, btree.Root.Children[4].Keys[3]);
            Assert.Equal(32, btree.Root.Children[4].Keys[4]);
        }

        [Fact]
        public void TestDelete(){
            BTree btree = CreateBTree(3);

            btree.Delete(1);
            Assert.Null(btree.Search(1));

            btree.Delete(10);
            Assert.Null(btree.Search(10));

            btree.Delete(14);
            Assert.Null(btree.Search(14));

            btree.Delete(6);
            Assert.Null(btree.Search(6));

            Assert.Equal(5, btree.Root.Keys[0]);
            Assert.Equal(8, btree.Root.Keys[1]);
            Assert.Equal(13, btree.Root.Keys[2]);
            Assert.Equal(19, btree.Root.Keys[3]);
            Assert.Equal(2, btree.Root.Children[0].Keys[0]);
            Assert.Equal(5, btree.Root.Children[0].Keys[1]);
            Assert.Equal(7, btree.Root.Children[1].Keys[0]);
            Assert.Equal(8, btree.Root.Children[1].Keys[1]);
            Assert.Equal(11, btree.Root.Children[2].Keys[0]);
            Assert.Equal(12, btree.Root.Children[2].Keys[1]);
            Assert.Equal(17, btree.Root.Children[3].Keys[0]);
            Assert.Equal(19, btree.Root.Children[3].Keys[1]);
            Assert.Equal(20, btree.Root.Children[4].Keys[0]);
            Assert.Equal(25, btree.Root.Children[4].Keys[1]);
            Assert.Equal(28, btree.Root.Children[4].Keys[2]);
            Assert.Equal(30, btree.Root.Children[4].Keys[3]);
            Assert.Equal(32, btree.Root.Children[4].Keys[4]);
        }

        [Fact]
        private void TestDeleteFill(){
            BTree btree = CreateBTree(3);

            btree.Delete(1);
            Assert.Null(btree.Search(1));

            btree.Delete(10);
            Assert.Null(btree.Search(10));

            btree.Delete(14);
            Assert.Null(btree.Search(14));

            btree.Delete(6);
            Assert.Null(btree.Search(6));

            btree.Delete(7);//
            Assert.Null(btree.Search(7));

            Assert.Equal(5, btree.Root.Keys[0]);
            Assert.Equal(11, btree.Root.Keys[1]);
            Assert.Equal(13, btree.Root.Keys[2]);
            Assert.Equal(19, btree.Root.Keys[3]);
            Assert.Equal(2, btree.Root.Children[0].Keys[0]);
            Assert.Equal(5, btree.Root.Children[0].Keys[1]);
            Assert.Equal(8, btree.Root.Children[1].Keys[0]);
            Assert.Equal(11, btree.Root.Children[1].Keys[1]);
            Assert.Equal(12, btree.Root.Children[2].Keys[0]);
            Assert.Equal(13, btree.Root.Children[2].Keys[1]);
            Assert.Equal(17, btree.Root.Children[3].Keys[0]);
            Assert.Equal(19, btree.Root.Children[3].Keys[1]);
            Assert.Equal(20, btree.Root.Children[4].Keys[0]);
            Assert.Equal(25, btree.Root.Children[4].Keys[1]);
            Assert.Equal(28, btree.Root.Children[4].Keys[2]);
            Assert.Equal(30, btree.Root.Children[4].Keys[3]);
            Assert.Equal(32, btree.Root.Children[4].Keys[4]);
        }

        [Fact]
        public void RangeQuery_EmptyTree_ReturnsEmptyList(){
            int degree = 3;
            BTree btree = new BTree(degree);

            List<int> result = btree.RangeQuery(1, 10);

            Assert.Empty(result);
        }

        [Fact]
        public void RangeQuery_NoKeysInRange_ReturnsEmptyList(){
            int degree = 3;
            BTree btree = new BTree(degree);
            int[] keysToInsert = { 10, 20, 5, 6, 12, 30, 7, 17 };
            foreach (var key in keysToInsert){
                btree.Insert(key);
            }

            List<int> result = btree.RangeQuery(21, 29);

            Assert.Empty(result);
        }

        [Fact]
        private void RangeQuery_ReturnsCorrectKeys(){
            BTree btree = CreateBTree(3);

            List<int> result = btree.RangeQuery(5, 20);//
            List<int> expected = new List<int>{5, 6, 7, 8, 10, 11, 12, 13, 14, 17, 19};
            
            for (int i = 0; i < result.Count - 1 ; i++){
                Assert.Equal(expected[i], result[i]);
            }
        }

        [Fact]
        private void Serialize_Deserialize_BTree_ShouldBeEqual(){
            BTree btree = CreateBTree(3);

            string serializedBTree = btree.Serialize();
            BTree deserializedBTree = BTree.Deserialize(serializedBTree);

            Assert.Equal(btree.Root.Keys, deserializedBTree.Root.Keys);
            Assert.Equal(btree.Root.Children.Count, deserializedBTree.Root.Children.Count);
        }


        private BTree CreateBTree(int degree){
            BTree btree = new BTree(degree);

            int[] keysToinsert = {10, 20, 5, 6, 12, 30, 7, 17, 19, 11, 2, 8, 1, 14, 32, 13, 25, 28};
            foreach (var key in keysToinsert){
                btree.Insert(key);
            }

            return btree;
        }
    }
}