using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BAKKA_Sharp
{
    public partial class LinearViewForm : Form
    {
        // Chart
        Chart _chart;
        internal Chart Chart
        {
            get => _chart;
            set
            {
                _chart = value;
                linearPanel.Invalidate();
            }
        }

        // Graphics
        BufferedGraphicsContext gfxContext;
        BufferedGraphics bufGraphics;
        LinearView linearView;

        public LinearViewForm()
        {
            InitializeComponent();

            // Form events
            linearPanel.MouseWheel += LinearPanel_MouseWheel;

            // Setup graphics
            gfxContext = BufferedGraphicsManager.Current;
            SetBufferedGraphicsContext();
            linearView = new LinearView(linearPanel.Size);

            // Force double buffering on linearPanel
            Type controlType = linearPanel.GetType();
            PropertyInfo pi = controlType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(linearPanel, true);
        }

        private void SetBufferedGraphicsContext()
        {
            gfxContext.MaximumBuffer = new Size(linearPanel.Width + 1, linearPanel.Height + 1);
            bufGraphics = gfxContext.Allocate(linearPanel.CreateGraphics(),
                new Rectangle(0, 0, linearPanel.Width, linearPanel.Height));
        }

        private void linearPanel_Paint(object sender, PaintEventArgs e)
        {
            Rectangle panelRect = new Rectangle(0, 0, linearPanel.Width, linearPanel.Height);
            float endMeasure = (float)Math.Ceiling(linearView.StartingMeasure + linearPanel.Height / (linearView.QuarterNoteHeight * 4));

            // Set drawing mode
            bufGraphics.Graphics.SmoothingMode = SmoothingMode.HighSpeed;

            // Draw background
            bufGraphics.Graphics.FillRectangle(new SolidBrush(Color.Black), panelRect);

            // Draw Lanes
            for (int i = 0; i < (linearView.NumLanes + 1); i++)
            {
                Pen lanePen;
                if (i % 15 == 0)
                    lanePen = linearView.MajorLanePen;
                else if (i % 5 == 0)
                    lanePen = linearView.MediumLanePen;
                else
                    lanePen = linearView.MinorLanePen;

                bufGraphics.Graphics.DrawLine(
                    lanePen,
                    linearView.LeftMargin + i * linearView.LaneWidth,
                    0,
                    linearView.LeftMargin + i * linearView.LaneWidth,
                    linearView.PanelSize.Height);
            }

            // Draw Measure Lines
            float startingRemainder = (float)Math.Ceiling(linearView.StartingMeasure) - linearView.StartingMeasure;
            float startingPoint = startingRemainder * linearView.QuarterNoteHeight * 4;
            for (int i = 0; i <= (int)(endMeasure - linearView.StartingMeasure); i++)
            {
                bufGraphics.Graphics.DrawLine(
                    linearView.MajorLanePen,
                    linearView.LeftMargin,
                    linearView.PanelSize.Height - startingPoint - i * linearView.QuarterNoteHeight * 4,
                    linearView.LeftMargin + linearView.AllLaneWidth,
                    linearView.PanelSize.Height - startingPoint - i * linearView.QuarterNoteHeight * 4);

                bufGraphics.Graphics.DrawString(
                    $"{(Math.Ceiling(linearView.StartingMeasure) + i):F0}",
                    linearView.GimmickFont,
                    linearView.LabelBrush,
                    linearView.LeftMargin - 8.0f,
                    linearView.PanelSize.Height - startingPoint - i * linearView.QuarterNoteHeight * 4 - 18.0f,
                    linearView.RightAlign);
            }

            // Draw Hi-Speed
            var hispeed = Chart.Gimmicks.Where(x => x.GimmickType == GimmickType.HiSpeedChange && x.Measure >= linearView.StartingMeasure && x.Measure <= endMeasure);
            foreach (var evt in hispeed)
            {
                float measureOffset = evt.Measure - (float)Math.Ceiling(linearView.StartingMeasure);
                float eventPoint = measureOffset * linearView.QuarterNoteHeight * 4;

                bufGraphics.Graphics.DrawLine(
                    linearView.HiSpeedPen,
                    linearView.LeftMargin + linearView.AllLaneWidth,
                    linearView.PanelSize.Height - startingPoint - eventPoint,
                    linearView.LeftMargin + linearView.AllLaneWidth + linearView.BpmMargin + linearView.TimeSigMargin + linearView.HiSpeedMargin,
                    linearView.PanelSize.Height - startingPoint - eventPoint);

                bufGraphics.Graphics.DrawString(
                    $"x {evt.HiSpeed:F3}",
                    linearView.GimmickFont,
                    linearView.HiSpeedBrush,
                    linearView.LeftMargin + linearView.AllLaneWidth + linearView.BpmMargin + linearView.TimeSigMargin + 8.0f,
                    linearView.PanelSize.Height - startingPoint - eventPoint - 18.0f);
            }

            // Draw Time Signature
            var timesig = Chart.Gimmicks.Where(x => x.GimmickType == GimmickType.TimeSignatureChange && x.Measure >= linearView.StartingMeasure && x.Measure <= endMeasure);
            foreach (var evt in timesig)
            {
                float measureOffset = evt.Measure - (float)Math.Ceiling(linearView.StartingMeasure);
                float eventPoint = measureOffset * linearView.QuarterNoteHeight * 4;

                bufGraphics.Graphics.DrawLine(
                    linearView.TimeSigPen,
                    linearView.LeftMargin + linearView.AllLaneWidth,
                    linearView.PanelSize.Height - startingPoint - eventPoint,
                    linearView.LeftMargin + linearView.AllLaneWidth + linearView.BpmMargin + linearView.TimeSigMargin,
                    linearView.PanelSize.Height - startingPoint - eventPoint);

                bufGraphics.Graphics.DrawString(
                    $"{evt.TimeSig.Upper}/{evt.TimeSig.Lower}",
                    linearView.GimmickFont,
                    linearView.TimeSigBrush,
                    linearView.LeftMargin + linearView.AllLaneWidth + linearView.BpmMargin + 8.0f,
                    linearView.PanelSize.Height - startingPoint - eventPoint - 18.0f);
            }

            // Draw BPM
            var bpm = Chart.Gimmicks.Where(x => x.GimmickType == GimmickType.BpmChange && x.Measure >= linearView.StartingMeasure && x.Measure <= endMeasure);
            foreach (var evt in bpm)
            {
                float measureOffset = evt.Measure - (float)Math.Ceiling(linearView.StartingMeasure);
                float eventPoint = measureOffset * linearView.QuarterNoteHeight * 4;

                bufGraphics.Graphics.DrawLine(
                    linearView.BpmPen,
                    linearView.LeftMargin + linearView.AllLaneWidth,
                    linearView.PanelSize.Height - startingPoint - eventPoint,
                    linearView.LeftMargin + linearView.AllLaneWidth + linearView.BpmMargin,
                    linearView.PanelSize.Height - startingPoint - eventPoint);

                bufGraphics.Graphics.DrawString(
                    evt.BPM.ToString("F2"),
                    linearView.GimmickFont,
                    linearView.BpmBrush,
                    linearView.LeftMargin + linearView.AllLaneWidth + 8.0f,
                    linearView.PanelSize.Height - startingPoint - eventPoint - 18.0f);
            }

            // Draw Selection Line
            float selectionOffset = linearView.SelectedMeasure - (float)Math.Ceiling(linearView.StartingMeasure);
            float selectionPoint = selectionOffset * linearView.QuarterNoteHeight * 4;

            bufGraphics.Graphics.DrawLine(
                linearView.SelectionPen,
                linearView.LeftMargin - 10.0f,
                linearView.PanelSize.Height - startingPoint - selectionPoint,
                linearView.LeftMargin + linearView.AllLaneWidth,
                linearView.PanelSize.Height - startingPoint - selectionPoint);

            // Draw Notes
            var drawNotes = Chart.Notes.Where(
                x => x.Measure >= linearView.StartingMeasure
                && x.Measure <= endMeasure
                && !x.IsHold
                && !x.IsMask).ToList();
            foreach (var note in drawNotes)
            {
                float measureOffset = note.Measure - (float)Math.Ceiling(linearView.StartingMeasure);
                float notePoint = (float)Math.Ceiling(measureOffset * linearView.QuarterNoteHeight * 4);

                int endLane = (44 - note.Position) < 0 ? (44 - note.Position) + 60 : (44 - note.Position);
                int size = note.Size;
                int startLane = (endLane - size + 1);
                int? startLane2 = null;
                int? size2 = null;
                if (startLane < 0)
                {
                    startLane2 = startLane + 60;
                    startLane = 0;
                    size = endLane + 1;
                    size2 = 60 - startLane2;
                }

                bufGraphics.Graphics.FillRectangle(
                    new SolidBrush(note.Color),
                    linearView.LeftMargin + linearView.LaneWidth * startLane + 1.0f,
                    linearView.PanelSize.Height - startingPoint - notePoint - 3.0f,
                    linearView.LaneWidth * size - 2.0f,
                    6.0f);

                if (startLane2 != null && size2 != null)
                { 
                    bufGraphics.Graphics.FillPolygon(
                        new SolidBrush(note.Color),
                        new PointF[] {
                            new PointF(linearView.LeftMargin - 8.0f, linearView.PanelSize.Height - startingPoint - notePoint + 1.0f),
                            new PointF(linearView.LeftMargin - 8.0f, linearView.PanelSize.Height - startingPoint - notePoint - 2.0f),
                            new PointF(linearView.LeftMargin + 1.0f, linearView.PanelSize.Height - startingPoint - notePoint - 4.0f),
                            new PointF(linearView.LeftMargin + 1.0f, linearView.PanelSize.Height - startingPoint - notePoint + 3.0f)
                        });

                    bufGraphics.Graphics.FillRectangle(
                        new SolidBrush(note.Color),
                        linearView.LeftMargin + linearView.LaneWidth * (int)startLane2 + 1.0f,
                        linearView.PanelSize.Height - startingPoint - notePoint - 3.0f,
                        linearView.LaneWidth * (int)size2 - 2.0f,
                        6.0f);

                    bufGraphics.Graphics.FillPolygon(
                        new SolidBrush(note.Color),
                        new PointF[] {
                            new PointF(linearView.LeftMargin + linearView.AllLaneWidth + 8.0f, linearView.PanelSize.Height - startingPoint - notePoint + 1.0f),
                            new PointF(linearView.LeftMargin + linearView.AllLaneWidth + 8.0f, linearView.PanelSize.Height - startingPoint - notePoint - 2.0f),
                            new PointF(linearView.LeftMargin + linearView.AllLaneWidth - 1.0f, linearView.PanelSize.Height - startingPoint - notePoint - 4.0f),
                            new PointF(linearView.LeftMargin + linearView.AllLaneWidth - 1.0f, linearView.PanelSize.Height - startingPoint - notePoint + 3.0f)
                        });
                }
            }

            bufGraphics.Render(e.Graphics);
        }

        private void LinearViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Hide();
                e.Cancel = true;
            }
        }

        private void linearPanel_Click(object sender, EventArgs e)
        {
            linearPanel.Invalidate();
        }

        private void LinearViewForm_Resize(object sender, EventArgs e)
        {
            SetBufferedGraphicsContext();
            linearView.Update(linearPanel.Size);
            linearPanel.Invalidate();
        }

        private void LinearPanel_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if (e.Delta > 0)
                    linearView.QuarterNoteHeight += 50;
                else if (linearView.QuarterNoteHeight > 50)
                    linearView.QuarterNoteHeight -= 50;
                linearPanel.Invalidate();
            }
            else if (Control.ModifierKeys == Keys.Alt)
            { }
            else if (Control.ModifierKeys == Keys.Shift)
            { }
            else
            {
                if (e.Delta > 0)
                {
                    linearView.StartingMeasure += 0.25f * (linearView.QuarterNoteHeight / 50.0f);
                }
                else
                {
                    if (linearView.StartingMeasure > -0.25f)
                        linearView.StartingMeasure = Math.Max(-0.25f, linearView.StartingMeasure - 0.25f * (linearView.QuarterNoteHeight / 50.0f));
                }
            }
            linearPanel.Invalidate();
        }
    }
}
