using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAKKA_Sharp
{
    internal class BeatInfo
    {
        public int Measure;
        public int Beat;

        public BeatInfo(int measure, int beat)
        {
            Measure = measure;
            Beat = beat;
        }

        public BeatInfo(float measure)
        {
            Measure = (int)Math.Floor(measure);
            Beat = (int)((measure - (float)Measure) * 1920.0f);
        }

        public BeatInfo(BeatInfo info)
        {
            Measure = info.Measure;
            Beat = info.Beat;
        }

        public float MeasureDecimal { get { return (float)Measure + (float)Beat / 1920.0f; } }
    }

    internal class TimeSignature
    {
        public int Upper;
        public int Lower;

        public double Ratio { get { return (double)Upper / (double)Lower; } }

        public TimeSignature()
        {
            Upper = 4;
            Lower = 4;
        }

        public TimeSignature(TimeSignature sig)
        {
            Upper = sig.Upper;
            Lower = sig.Lower;
        }
    }

    internal class NoteBase
    {
        public BeatInfo BeatInfo { get; set; } = new BeatInfo(-1, 0);
        public GimmickType GimmickType { get; set; } = GimmickType.NoGimmick;

        public float Measure { get { return BeatInfo.MeasureDecimal; } }
    }

    internal class Note : NoteBase
    {
        public NoteType NoteType { get; set; } = NoteType.TouchNoBonus;
        public int Position { get; set; }
        public int Size { get; set; }
        [System.ComponentModel.Browsable(false)]
        public bool HoldChange { get; set; }
        [System.ComponentModel.Browsable(false)]
        public MaskType MaskFill { get; set; }
        [System.ComponentModel.Browsable(false)]
        public Note NextNote { get; set; }
        [System.ComponentModel.Browsable(false)]
        public Note PrevNote { get; set; }

        [System.ComponentModel.Browsable(false)]
        public bool IsHold
        {
            get
            {
                switch (NoteType)
                {
                    case NoteType.HoldStartNoBonus:
                    case NoteType.HoldJoint:
                    case NoteType.HoldEnd:
                    case NoteType.HoldStartBonusFlair:
                        return true;
                    default:
                        return false;
                }
            }
        }
        [System.ComponentModel.Browsable(false)]
        public bool IsMask
        {
            get
            {
                switch (NoteType)
                {
                    case NoteType.MaskAdd:
                    case NoteType.MaskRemove:
                        return true;
                    default:
                        return false;
                }
            }
        }
        [System.ComponentModel.Browsable(false)]
        public bool IsBonus
        {
            get
            {
                switch (NoteType)
                {
                    case NoteType.TouchBonus:
                    case NoteType.SlideOrangeBonus:
                    case NoteType.SlideGreenBonus:
                        return true;
                    default:
                        return false; ;
                }
            }
        }
        [System.ComponentModel.Browsable(false)]
        public bool IsFlair
        {
            get
            {
                switch (NoteType)
                {
                    case NoteType.TouchBonusFlair:
                    case NoteType.SnapRedBonusFlair:
                    case NoteType.SnapBlueBonusFlair:
                    case NoteType.SlideOrangeBonusFlair:
                    case NoteType.SlideGreenBonusFlair:
                    case NoteType.HoldStartBonusFlair:
                    case NoteType.ChainBonusFlair:
                        return true;
                    default:
                        return false;
                }
            }
        }
        [System.ComponentModel.Browsable(false)]
        public Color Color
        {
            get
            {
                return Utils.NoteTypeToColor(NoteType);
            }
        }

        public Note() { }

        public Note(NoteBase baseNote)
        {
            BeatInfo = baseNote.BeatInfo;
            GimmickType = baseNote.GimmickType;
        }
    }

    internal class Gimmick : NoteBase
    {   
        public double BPM { get; set; }
        public TimeSignature TimeSig { get; set; } = new();
        public double HiSpeed { get; set; }
        public double StartTime { get; set; }

        public bool IsReverse
        {
            get
            {
                switch (GimmickType)
                {
                    case GimmickType.ReverseStart:
                    case GimmickType.ReverseMiddle:
                    case GimmickType.ReverseEnd:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsStop
        {
            get
            {
                switch (GimmickType)
                {
                    case GimmickType.StopStart:
                    case GimmickType.StopEnd:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public Gimmick() { }
        public Gimmick(NoteBase baseNote)
        {
            BeatInfo = baseNote.BeatInfo;
            GimmickType = baseNote.GimmickType;
        }
    }

    internal class Chart
    {
        public List<Note> Notes { get; set; }
        public List<Gimmick> Gimmicks { get; set; }
        /// <summary>
        /// Offset in seconds.
        /// </summary>
        public double Offset { get; set; }
        /// <summary>
        /// Movie offset in seconds
        /// </summary>
        public double MovieOffset { get; set; }
        String SongFileName { get; set; }
        List<Gimmick> TimeEvents { get; set; }
        public bool HasInitEvents
        {
            get
            {
                return TimeEvents != null &&
                    TimeEvents.Count > 0 &&
                    Gimmicks.Count(x => x.Measure == 0 && x.GimmickType == GimmickType.BpmChange) >= 1 &&
                    Gimmicks.Count(x => x.Measure == 0 && x.GimmickType == GimmickType.TimeSignatureChange) >= 1;
            }
        }

        public Chart()
        {
            Notes = new List<Note>();
            Gimmicks = new List<Gimmick>();
            Offset = 0;
            MovieOffset = 0;
            SongFileName = "";
        }

        public bool ParseFile(string filename)
        {
            var file = File.ReadAllLines(filename);

            int index = 0;

            do
            {
                var line = file[index];

                var path = Utils.GetTag(line, "#MUSIC_FILE_PATH ");
                if (path != null)
                    SongFileName = path;

                var offset = Utils.GetTag(line, "#OFFSET");
                if (offset != null)
                    Offset = Convert.ToDouble(offset);

                offset = Utils.GetTag(line, "#MOVIEOFFSET");
                if (offset != null)
                    MovieOffset = Convert.ToDouble(offset);


                if (line.Contains("#BODY"))
                {
                    index++;
                    break;
                }

            } while(++index < file.Length);

            double beat;
            double subbeat;
            int type;
            int lineNum;
            Gimmick gimmickTemp;
            Note noteTemp;
            Dictionary<int, Note> notesByLine = new();
            Dictionary<int, int> refByLine = new();
            for (int i = index; i < file.Length; i++)
            {
                var parsed = file[i].Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                NoteBase temp = new();
                temp.BeatInfo = new BeatInfo(Convert.ToInt32(parsed[0]), Convert.ToInt32(parsed[1]));
                temp.GimmickType = (GimmickType)Convert.ToInt32(parsed[2]);

                switch (temp.GimmickType)
                {
                    case GimmickType.NoGimmick:
                        noteTemp = new Note(temp);
                        noteTemp.NoteType = (NoteType)Convert.ToInt32(parsed[3]);
                        lineNum = Convert.ToInt32(parsed[4]);
                        noteTemp.Position = Convert.ToInt32(parsed[5]);
                        noteTemp.Size = Convert.ToInt32(parsed[6]);
                        noteTemp.HoldChange = Convert.ToBoolean(Convert.ToInt32(parsed[7]));
                        if (noteTemp.NoteType == NoteType.MaskAdd || noteTemp.NoteType == NoteType.MaskRemove)
                        {
                            noteTemp.MaskFill = (MaskType)Convert.ToInt32(parsed[8]);
                        }
                        else if (noteTemp.NoteType == NoteType.HoldStartNoBonus ||
                            noteTemp.NoteType == NoteType.HoldJoint ||
                            noteTemp.NoteType == NoteType.HoldStartBonusFlair)
                        {
                            refByLine[lineNum] = Convert.ToInt32(parsed[8]);
                        }
                        Notes.Add(noteTemp);
                        notesByLine[lineNum] = Notes.Last();
                        break;
                    case GimmickType.BpmChange:
                        gimmickTemp = new Gimmick(temp);
                        gimmickTemp.BPM = Convert.ToDouble(parsed[3]);
                        Gimmicks.Add(gimmickTemp);
                        break;
                    case GimmickType.TimeSignatureChange:
                        gimmickTemp = new Gimmick(temp);
                        gimmickTemp.TimeSig = new TimeSignature() { Upper = Convert.ToInt32(parsed[3]), Lower = Convert.ToInt32(parsed[4]) };
                        Gimmicks.Add(gimmickTemp);
                        break;
                    case GimmickType.HiSpeedChange:
                        gimmickTemp = new Gimmick(temp);
                        gimmickTemp.HiSpeed = Convert.ToDouble(parsed[3]);
                        Gimmicks.Add(gimmickTemp);
                        break;
                    case GimmickType.ReverseStart:
                    case GimmickType.ReverseMiddle:
                    case GimmickType.ReverseEnd:
                    case GimmickType.StopStart:
                    case GimmickType.StopEnd:
                    default:
                        Gimmicks.Add(new Gimmick(temp));
                        break;
                }

            }

            // Generate hold references
            for (int i = 0; i < Notes.Count; i++)
            {
                if (refByLine.ContainsKey(i))
                {
                    Notes[i].NextNote = notesByLine[refByLine[i]];
                    Notes[i].NextNote.PrevNote = Notes[i];
                }
            }

            RecalcTime();

            return true;
        }

        public bool WriteFile(string filename)
        {
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create), new UTF8Encoding(false)))
            {
                // LF line ending
                sw.NewLine = "\n";

                sw.WriteLine("#MUSIC_SCORE_ID 0");
                sw.WriteLine("#MUSIC_SCORE_VERSION 0");
                sw.WriteLine("#GAME_VERSION ");
                sw.WriteLine($"#MUSIC_FILE_PATH {SongFileName}");
                sw.WriteLine($"#OFFSET {Offset:F6}");
                sw.WriteLine($"#MOVIEOFFSET {MovieOffset:F6}");
                sw.WriteLine("#BODY");

                foreach (var gimmick in Gimmicks)
                {
                    sw.Write($"{gimmick.BeatInfo.Measure,4:F0}{gimmick.BeatInfo.Beat,5:F0}{((int)gimmick.GimmickType),5:F0}");
                    switch (gimmick.GimmickType)
                    {
                        case GimmickType.BpmChange:
                            sw.WriteLine($" {gimmick.BPM:F6}");
                            break;
                        case GimmickType.TimeSignatureChange:
                            sw.WriteLine($"{gimmick.TimeSig.Upper,5:F0}{gimmick.TimeSig.Lower,5:F0}");
                            break;
                        case GimmickType.HiSpeedChange:
                            sw.WriteLine($" {gimmick.HiSpeed:F6}");
                            break;
                        default:
                            sw.WriteLine("");
                            break;
                    }
                }

                foreach (var note in Notes)
                {
                    sw.Write($"{note.BeatInfo.Measure,4:F0}{note.BeatInfo.Beat,5:F0}{((int)note.GimmickType),5:F0}{(int)note.NoteType,5:F0}");
                    sw.Write($"{Notes.IndexOf(note),5:F0}{note.Position,5:F0}{note.Size,5:F0}{Convert.ToInt32(note.HoldChange),5:F0}");
                    if (note.IsMask)
                        sw.Write($"{(int)note.MaskFill,5:F0}");
                    if (note.NextNote != null)
                        sw.Write($"{Notes.IndexOf(note.NextNote),5:F0}");
                    sw.WriteLine("");
                }
            }

            return true;
        }

        public void RecalcTime()
        {
            Gimmicks.Sort((x, y) => x.Measure.CompareTo(y.Measure));
            var timeSig = Gimmicks.FirstOrDefault(x => x.GimmickType == GimmickType.TimeSignatureChange && x.Measure == 0.0f);
            var bpm = Gimmicks.FirstOrDefault(x => x.GimmickType == GimmickType.BpmChange && x.Measure == 0.0f);
            if (timeSig == null || bpm == null)
                return;     // Cannot calculate times without either starting value

            TimeEvents = new();
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                var evt = TimeEvents.FirstOrDefault(x => x.BeatInfo.MeasureDecimal == Gimmicks[i].BeatInfo.MeasureDecimal);

                if (Gimmicks[i].GimmickType == GimmickType.BpmChange)
                {
                    if (evt == null)
                    {
                        TimeEvents.Add(new Gimmick()
                        {
                            BeatInfo = new BeatInfo(Gimmicks[i].BeatInfo),
                            BPM = Gimmicks[i].BPM,
                            TimeSig = new TimeSignature(timeSig.TimeSig)
                        });
                    }
                    else
                    {
                        evt.BPM = Gimmicks[i].BPM;
                    }
                    bpm = Gimmicks[i];
                }
                if (Gimmicks[i].GimmickType == GimmickType.TimeSignatureChange)
                {
                    if (evt == null)
                    {
                        TimeEvents.Add(new Gimmick()
                        {
                            BeatInfo = new BeatInfo(Gimmicks[i].BeatInfo),
                            BPM = bpm.BPM,
                            TimeSig = new TimeSignature(Gimmicks[i].TimeSig)
                        });
                    }
                    else
                    {
                        evt.TimeSig = new TimeSignature(Gimmicks[i].TimeSig);
                    }
                    timeSig = Gimmicks[i];
                }
            }

            // Run through all time events and generate valid start times
            TimeEvents[0].StartTime = Offset * 1000.0;
            for (int i = 1; i < TimeEvents.Count; i++)
            {
                TimeEvents[i].StartTime = ((TimeEvents[i].Measure - TimeEvents[i - 1].Measure) * 4 * TimeEvents[i - 1].TimeSig.Ratio * 60000.0 / TimeEvents[i].BPM) + TimeEvents[i - 1].StartTime;
            }
        }

        /// <summary>
        /// Translate clock time to beats
        /// </summary>
        /// <param name="time">Current timestamp in ms</param>
        /// <returns></returns>
        public BeatInfo GetBeat(float time)
        {
            if (TimeEvents == null || TimeEvents.Count == 0)
                return new BeatInfo(-1, 0);

            var evt = TimeEvents.Where(x => time >= x.StartTime).LastOrDefault();
            if (evt == null)
                evt = TimeEvents[0];
            return new BeatInfo((float)(evt.BPM * (time - evt.StartTime) / (60000.0f * evt.TimeSig.Ratio * 4)) + evt.Measure);
        }

        /// <summary>
        /// Translate measures into clock time
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        public int GetTime(BeatInfo beat)
        {
            if (TimeEvents == null || TimeEvents.Count == 0)
                return 0;

            var evt = TimeEvents.Where(x => beat.MeasureDecimal >= x.Measure).LastOrDefault();
            if (evt == null)
                evt = TimeEvents[0];
            return (int)((60000.0 * 4.0 * evt.TimeSig.Ratio / evt.BPM) * (beat.MeasureDecimal - evt.Measure) + evt.StartTime);
        }
    }
}
