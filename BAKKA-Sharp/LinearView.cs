using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAKKA_Sharp
{
    internal class LinearView
    {
        public SizeF PanelSize { get; private set; }
        public int LaneWidth { get; private set; }
        public int LeftMargin { get; set; }
        public int AllLaneWidth { get; private set; }
        public int BpmMargin { get; private set; }
        public float StartingMeasure { get; set; } = -0.25f;
        public float SelectedMeasure { get; set; } = 0.0f;

        public int QuarterNoteHeight { get; } = 50;
        public int NumLanes { get; } = 60;

        public Pen LanePen { get; private set; }
        public Pen BpmPen { get; } = new Pen(Color.Lime, 1.0f);
        public Brush BpmBrush { get; } = new SolidBrush(Color.Lime);
        public Font GimmickBrush { get; } = new Font("Arial", 12.0f);

        public LinearView(SizeF size)
        {
            Update(size);
        }

        public void Update(SizeF size)
        {
            PanelSize = size;

            LeftMargin = (int)(PanelSize.Width * 0.15f);
            LaneWidth = (int)(PanelSize.Width * 0.7f / NumLanes);
            AllLaneWidth = LaneWidth * NumLanes;
            BpmMargin = (int)(PanelSize.Width * 0.08f);

            LanePen = new Pen(Color.FromArgb(42, 42, 42), 1.0f);
        }
    }
}
