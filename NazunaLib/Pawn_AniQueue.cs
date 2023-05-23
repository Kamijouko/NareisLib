using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NareisLib
{
    public class Pawn_AniQueue
    {
        public Pawn_AniQueue nextQueue;

        public bool loop;
        public int count;
        public int curFrame;
        public int startFrame;
        public int endFrame;
        public int frameCount;
        public List<DAL_GraphicInfo>[] framePack;

        public bool Ended
        {
            get
            {
                return curFrame == endFrame;
            }
        }

        public Pawn_AniQueue(int start = 0, int end = 0, bool loop = false, List<DAL_GraphicInfo>[] pack = null, Pawn_AniQueue next = null)
        {
            count = start;
            curFrame = start;
            startFrame = start;
            endFrame = end;
            this.loop = loop;
            if (pack != null)
            {
                if (end == 0)
                {
                    endFrame = pack.Count() - 1;
                }
                frameCount = pack.Count();
                framePack = pack;
            }
            nextQueue = next;
        }

        public bool ExecuteQueue(out List<DAL_GraphicInfo> data, bool pause = false, bool stop = false, bool next = false, bool next2 = false, bool next3 = false)
        {
            if (!Ended || !next)
            {
                if (!pause)
                {
                    count++;
                }
                curFrame = !pause ? count - 1 : count;
                data = framePack[curFrame];
                if (loop && Ended && !next)
                {
                    count = startFrame;
                    curFrame = startFrame;
                }
            }
            else
            {
                return nextQueue.ExecuteQueue(out data, pause, stop, next2, next3, false);
            }

            return ((Ended && nextQueue == null) || stop) ? true : false;
        }
    }
}
