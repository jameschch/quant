using QuantConnect.Data;

namespace GeneticTree.Signal
{
    public abstract class SignalBase : ISignal
    {

        public ISignal Child { get; set; }

        public ISignal Parent { get; set; }

        public Operator Operator { get; set; }

        public abstract bool IsReady { get; }

        public abstract string Name { get; }

        public abstract bool IsTrue();

        public virtual void Update(BaseData data)
        {
            if (Child != null)
            {
                Child.Update(data);
            }
        }

    }
}
