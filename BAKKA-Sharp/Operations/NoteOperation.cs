using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAKKA_Sharp.Operations
{
    internal abstract class NoteOperation : IOperation
    {
        protected Note Note { get; }
        protected Chart Chart { get; }
        public abstract string Description { get; }

        public NoteOperation(Chart chart, Note item)
        {
            Chart = chart;
            Note = item;
        }

        public abstract void Redo();

        public abstract void Undo();
    }

    internal class InsertNote : NoteOperation
    {
        public override string Description => "Insert note";

        public InsertNote(Chart chart, Note item) : base(chart, item)
        { }

        public override void Redo()
        {
            Chart.Notes.Add(Note);
        }

        public override void Undo()
        {
            Chart.Notes.Remove(Note);
        }
    }

    internal class RemoveNote : NoteOperation
    {
        public override string Description => "Remove note";

        public RemoveNote(Chart chart, Note item) : base(chart, item)
        { }

        public override void Redo()
        {
            Chart.Notes.Remove(Note);
        }

        public override void Undo()
        {
            Chart.Notes.Add(Note);
        }
    }
}
