using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Gridsum.DataflowEx;

namespace DataflowEx_Playground
{
    public class AggregatorFlow : Dataflow<string>
    {
        //Blocks
        private TransformBlock<string, KeyValuePair<string, int>> _splitter;
        private ActionBlock<KeyValuePair<string, int>> _aggregater;

        //Data
        private Dictionary<string, int> _dict;

        public AggregatorFlow() : base(DataflowOptions.Default)
        {
            _splitter = new TransformBlock<string, KeyValuePair<string, int>>(s => this.Split(s));
            _dict = new Dictionary<string, int>();
            _aggregater = new ActionBlock<KeyValuePair<string, int>>(p => this.Aggregate(p));

            //Block linking
            _splitter.LinkTo(_aggregater, new DataflowLinkOptions() { PropagateCompletion = true });

            /* IMPORTANT */
            RegisterChild(_splitter);
            RegisterChild(_aggregater);
        }

        protected virtual void Aggregate(KeyValuePair<string, int> pair)
        {
            int oldValue;
            _dict[pair.Key] = this._dict.TryGetValue(pair.Key, out oldValue) ? oldValue + pair.Value : pair.Value;
        }

        protected virtual KeyValuePair<string, int> Split(string input)
        {
            string[] splitted = input.Split('=');
            return new KeyValuePair<string, int>(splitted[0], int.Parse(splitted[1]));
        }

        public override ITargetBlock<string> InputBlock { get { return _splitter; } }

        public IDictionary<string, int> Result { get { return _dict; } }
    }

}
