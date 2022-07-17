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
        public int TimeSigMargin { get; private set; }
        public int HiSpeedMargin { get; private set; }
        public float StartingMeasure { get; set; } = -0.25f;
        public float SelectedMeasure { get; set; } = 0.0f;

        public int QuarterNoteHeight { get; set; } = 50;
        public int NumLanes { get; } = 60;

        public Pen MinorLanePen { get; } = new Pen(Color.FromArgb(42, 42, 42), 1.0f);
        public Pen MediumLanePen { get; } = new Pen(Color.FromArgb(80, 80, 80), 1.0f);
        public Pen MajorLanePen { get; } = new Pen(Color.FromArgb(100, 100, 100), 1.0f);
        public Brush LabelBrush { get; } = new SolidBrush(Color.FromArgb(204, 204, 204));

        public Pen SelectionPen { get; } = new Pen(Color.Red, 1.0f);

        public Pen BpmPen { get; } = new Pen(Color.Lime, 1.0f);
        public Brush BpmBrush { get; } = new SolidBrush(Color.Lime);
        public Pen TimeSigPen { get; } = new Pen(Color.Orange, 1.0f);
        public Brush TimeSigBrush { get; } = new SolidBrush(Color.Orange);
        public Pen HiSpeedPen { get; } = new Pen(Color.Salmon, 1.0f);
        public Brush HiSpeedBrush { get; } = new SolidBrush(Color.Salmon);

        public Font GimmickFont { get; } = new Font("Arial", 10.0f);
        public StringFormat RightAlign { get; } = new StringFormat() { Alignment = StringAlignment.Far };

        public LinearView(SizeF size)
        {
            Update(size);
        }

        public void Update(SizeF size)
        {
            PanelSize = size;

            LeftMargin = (int)(PanelSize.Width * 0.08f);
            LaneWidth = (int)Math.Max(8, PanelSize.Width * 0.68f / NumLanes);
            AllLaneWidth = LaneWidth * NumLanes;
            BpmMargin = TimeSigMargin = HiSpeedMargin = (int)Math.Max(60, PanelSize.Width * 0.08f);
        }
    }
}
