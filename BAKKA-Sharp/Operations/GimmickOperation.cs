using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAKKA_Sharp.Operations
{
    internal abstract class GimmickOperation : IOperation
    {
        protected Gimmick Gimmick { get; }
        protected Chart Chart { get; }
        public abstract string Description { get; }

        public GimmickOperation(Chart chart, Gimmick item)
        {
            Chart = chart;
            Gimmick = item;
        }

        public abstract void Redo();

        public abstract void Undo();
    }

    internal class InsertGimmick : GimmickOperation
    {
        public override string Description => "Insert gimmick";

        public InsertGimmick(Chart chart, Gimmick item) : base(chart, item)
        { }

        public override void Redo()
        {
            Chart.Gimmicks.Add(Gimmick);
        }

        public override void Undo()
        {
            Chart.Gimmicks.Remove(Gimmick);
        }
    }

    internal class RemoveGimmick : GimmickOperation
    {
        public override string Description => "Remove gimmick";

        public RemoveGimmick(Chart chart, Gimmick item) : base(chart, item)
        { }

        public override void Redo()
        {
            Chart.Gimmicks.Remove(Gimmick);
        }

        public override void Undo()
        {
            Chart.Gimmicks.Add(Gimmick);
        }
    }
}
