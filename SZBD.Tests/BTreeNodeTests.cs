using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorageLayer.Indexes;

namespace SZBD.Tests
{
    public class BTreeNodeTest
    {
        [Fact]
        public void Insert_ShouldAddKeyToTree(){
            var node = new BTreeNode(true, 3);

            node.Insert(10);
            node.Insert(20);
            node.Insert(5);

            Assert.Equal(3, node.Keys.Count);
            Assert.Contains(10, node.Keys);
            Assert.Contains(20, node.Keys);
            Assert.Contains(5, node.Keys);
        }

        [Fact]
        public void Insert_ShouldSplitRootWhenFull(){
            var node = new BTreeNode(true, 3);

            node.Insert(10);//
            node.Insert(20);
            node.Insert(5);
            node.Insert(6);
            node.Insert(2);
            node.Insert(1);
            node.Insert(3);
            node.Insert(4);
            node.Insert(7);
            node.Insert(21);
            node.Insert(22);
            node.Insert(30);
            node.Insert(11);

            Assert.Equal(new List<int> { 3, 6, 20 }, node.Keys);
            Assert.Equal(new List<int> { 1, 2 }, node.Children[0].Keys);
            Assert.Equal(new List<int> { 4, 5 }, node.Children[1].Keys);
            Assert.Equal(new List<int> { 7, 10, 11 }, node.Children[2].Keys);
            Assert.Equal(new List<int> { 21, 22, 30 }, node.Children[3].Keys);   
        }

        [Fact]
        public void Search_ShouldReturnNodeIfKeyExists()
        {
            var node = new BTreeNode(true, 3);
            node.Insert(10);
            node.Insert(20);
            node.Insert(5);

            var result = node.Search(20);

            Assert.NotNull(result);
            Assert.Contains(20, result.Keys);
        }

        [Fact]
        public void Search_ShouldReturnNullIfKeyDoesNotExist()
        {
            var node = new BTreeNode(true, 3);
            node.Insert(10);
            node.Insert(20);
            node.Insert(5);

            var result = node.Search(15);

            Assert.Null(result);
        }

        [Fact]
        public void SplitChild_ShouldSplitFullChild()
        {
            var node = new BTreeNode(false, 2);
            var child = new BTreeNode(true, 2);
            child.Keys.AddRange(new List<int> { 1, 2, 3 });
            node.Children.Add(child);

            node.SplitChild(0, child);

            Assert.Equal(1, node.Keys.Count);
            Assert.Equal(2, node.Keys[0]);
            Assert.Equal(2, node.Children.Count);
            Assert.Equal(1, node.Children[0].Keys.Count);
            Assert.Equal(1, node.Children[1].Keys.Count);
        }
    }
}
