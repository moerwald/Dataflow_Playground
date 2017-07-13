using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataflowEx_Playground
{
    [TestFixture]
    class LineAggregatorFlowTest
    {

        /// <summary>
        /// Tests nesting of flow LineAggregatorFlow uses AggregatorFlow as child
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestFlow()
        {
            var lineAggregator = new LineAggregatorFlow();
            await lineAggregator.ProcessAsync(new[] { "a=1 b=2 a=5", "c=6 b=8" });
            Assert.IsTrue(lineAggregator["a"] == 6);
        }
    }
}
