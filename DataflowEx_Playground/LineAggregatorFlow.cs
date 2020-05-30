namespace DataflowEx_Playground
{
    using System.Threading.Tasks.Dataflow;
    using Gridsum.DataflowEx;

    public class LineAggregatorFlow : Dataflow<string>
    {
        private Dataflow<string, string> _lineProcessor;
        private AggregatorFlow _aggregator;
        public LineAggregatorFlow() : base(DataflowOptions.Default)
        {
            _lineProcessor = new TransformManyBlock<string, string>(line => line.Split(' ')).ToDataflow();
            _aggregator = new AggregatorFlow();
            _lineProcessor.LinkTo(_aggregator);
            RegisterChild(_lineProcessor);
            RegisterChild(_aggregator);
        }

        public override ITargetBlock<string> InputBlock { get { return _lineProcessor.InputBlock; } }
        public int this[string key] { get { return _aggregator.Result[key]; } }
    }
}
