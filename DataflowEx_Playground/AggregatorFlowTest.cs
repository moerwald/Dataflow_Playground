using System.Threading.Tasks;
using NUnit.Framework;

namespace DataflowEx_Playground
{
    [TestFixture]
    public class AggregatorFlowTest
    {
        [Test]
        public async Task TestFlow()
        {
            var aggregatorFlow = new AggregatorFlow();
            await aggregatorFlow.ProcessAsync(new[] { "a=1", "b=2", "a=5" });
            await aggregatorFlow.CompletionTask;
            Assert.IsTrue(aggregatorFlow.Result["a"] == 6); 
        }
    }
}
