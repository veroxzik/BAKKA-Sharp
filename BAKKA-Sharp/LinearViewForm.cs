using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            float endMeasure = linearPanel.Height / (linearView.QuarterNoteHeight * 4);

            // Set drawing mode
            bufGraphics.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            // Draw background
            bufGraphics.Graphics.FillRectangle(new SolidBrush(Color.Black), panelRect);

            // Draw Lanes
            for (int i = 0; i < (linearView.NumLanes + 1); i++)
            {
                bufGraphics.Graphics.DrawLine(
                    linearView.LanePen, 
                    linearView.LeftMargin + i * linearView.LaneWidth, 
                    0, 
                    linearView.LeftMargin + i * linearView.LaneWidth, 
                    linearView.PanelSize.Height);
            }

            // Draw Measure Lines
            float startingRemainder = (float)Math.Ceiling(linearView.StartingMeasure) - linearView.StartingMeasure;
            float startingPoint = startingRemainder * linearView.QuarterNoteHeight * 4;
            for (int i = 0; i < 10; i++)
            {
                bufGraphics.Graphics.DrawLine(
                    linearView.LanePen,
                    linearView.LeftMargin,
                    linearView.PanelSize.Height - startingPoint - i * linearView.QuarterNoteHeight * 4,
                    linearView.LeftMargin + linearView.AllLaneWidth,
                    linearView.PanelSize.Height - startingPoint - i * linearView.QuarterNoteHeight * 4);
            }

            // Draw BPM
            var bpm = Chart.Gimmicks.Where(x => x.GimmickType == GimmickType.BpmChange && x.Measure >= linearView.StartingMeasure && x.Measure <= endMeasure);
            foreach (var evt in bpm)
            {
                float measureOffset = (float)Math.Ceiling(evt.Measure) - (float)Math.Ceiling(linearView.StartingMeasure);
                float eventPoint = measureOffset * linearView.QuarterNoteHeight * 4;

                bufGraphics.Graphics.DrawLine(
                    linearView.BpmPen,
                    linearView.LeftMargin + linearView.AllLaneWidth,
                    linearView.PanelSize.Height - startingPoint - eventPoint,
                    linearView.LeftMargin + linearView.AllLaneWidth + linearView.BpmMargin,
                    linearView.PanelSize.Height - startingPoint - eventPoint);
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
            { }
            else if (Control.ModifierKeys == Keys.Alt)
            { }
            else if (Control.ModifierKeys == Keys.Shift)
            { }
            else
            {
                if (e.Delta > 0)
                {
                    linearView.StartingMeasure += 0.25f;
                }
                else
                {
                    if (linearView.StartingMeasure > -0.25f)
                        linearView.StartingMeasure -= 0.25f;

                }
            }
            linearPanel.Invalidate();
        }
    }
}
