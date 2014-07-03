using NUnit.Framework;

namespace Gem.DependencyInjector.Test
{
    [TestFixture]
    public class ParallelQuickSortTest
    {
        [Test]
        public void TestParallelQuickSort()
        {
            int max = 3000;
            int[] numbers = new int[max];
            for (int i = 0; i < max; i++)
            {
                numbers[i] = max - i;
            }
            int[] sorted = ParallelQuickSort.Sort(numbers);
            Assert.AreEqual(1, sorted[0]);
            Assert.AreEqual(max, sorted[max-1]);
        }

        [Test]
        public void TestSequentialQuickSort()
        {
            int max = 1000;
            int[] numbers = new int[max];
            for (int i = 0; i < max; i++)
            {
                numbers[i] = max - i;
            }
            int[] sorted = ParallelQuickSort.Sort(numbers);
            Assert.AreEqual(1, sorted[0]);
            Assert.AreEqual(max, sorted[max - 1]);
        }

        [Test]
        public void TestDirectSequentialQuickSort()
        {
            int max = 1000;
            int[] numbers = new int[max];
            for (int i = 0; i < max; i++)
            {
                numbers[i] = max - i;
            }
            ParallelQuickSort.SequentialSort(numbers);
            Assert.AreEqual(1, numbers[0]);
            Assert.AreEqual(max, numbers[max - 1]);
        }
    }
}