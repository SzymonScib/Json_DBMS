using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorageLayer.Indexes;

namespace SZBD.Tests
{
    public class BTreeTests
    {
        [Fact]
        public void TestInsert()
        {
            int degree = 3;
            BTree btree = new BTree(degree);

            btree.Insert(10);//
            btree.Insert(20);//
            btree.Insert(5);//
            btree.Insert(6);//
            btree.Insert(12);//
            btree.Insert(30);//
            btree.Insert(7);
            btree.Insert(17);
            btree.Insert(19);
            btree.Insert(11);
            btree.Insert(2);
            btree.Insert(8);
            btree.Insert(1);
            btree.Insert(14);
            btree.Insert(32);
            btree.Insert(13);
            btree.Insert(25);
            btree.Insert(28);

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
    }
}