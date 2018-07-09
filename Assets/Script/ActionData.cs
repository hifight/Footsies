using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
    public abstract class FrameDataBase
    {
        public Vector2Int startEndFrame;
    }
    [System.Serializable]
    public class MotionFrameData : FrameDataBase
    {
        public int motionID;
    }

    [System.Serializable]
    public class StatusData : FrameDataBase
    {
        public bool counterHit;
    }


    [System.Serializable]
    public class HitboxData : FrameDataBase
    {
        public Rect rect;
        public int attackID;
        public bool proximity;
    }

    [System.Serializable]
    public class HurtboxData : FrameDataBase
    {
        public Rect rect;
        public bool useBaseRect;
    }

    [System.Serializable]
    public class PushboxData : FrameDataBase
    {
        public Rect rect;
        public bool useBaseRect;
    }
    
    [System.Serializable]
    public class MovementData : FrameDataBase
    {
        public float velocity_x;
    }

    [System.Serializable]
    public class CancelData : FrameDataBase
    {
        public bool buffer;
        public bool execute;
        public List<int> actionID = new List<int>();
    }

    public enum ActionType
    {
        Movement,
        Attack,
        Damage,
        Guard,
    }

    [CreateAssetMenu]
    public class ActionData : ScriptableObject
    {
        public int actionID;
        public string actionName;
        public ActionType Type;
        public int frameCount;
        public bool isLoop;
        public int loopFromFrame;
        public MotionFrameData[] motions;
        public StatusData[] status;
        public HitboxData[] hitboxes;
        public HurtboxData[] hurtboxes;
        public PushboxData[] pushboxes;
        public MovementData[] movements;
        public CancelData[] cancels;
        public bool alwaysCancelable;
        public AudioClip audioClip;

        public MotionFrameData GetMotionData(int frame)
        {
            foreach(var data in motions)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    return data;
            }

            return null;
        }

        public StatusData GetStatusData(int frame)
        {
            foreach (var data in status)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    return data;
            }

            return null;
        }

        public List<HitboxData> GetHitboxData(int frame)
        {
            var hb = new List<HitboxData>();
            
            foreach (var data in this.hitboxes)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    hb.Add(data);
            }

            return hb;
        }

        public List<HurtboxData> GetHurtboxData(int frame)
        {
            var hb = new List<HurtboxData>();

            foreach (var data in this.hurtboxes)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    hb.Add(data);
            }

            return hb;
        }

        public PushboxData GetPushboxData(int frame)
        {
            foreach (var data in this.pushboxes)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    return data;
            }

            return null;
        }

        public MovementData GetMovementData(int frame)
        {
            foreach (var data in this.movements)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    return data;
            }

            return null;
        }

        public List<CancelData> GetCancelData(int frame)
        {
            var cd = new List<CancelData>();

            foreach (var data in this.cancels)
            {
                if (frame >= data.startEndFrame.x && frame <= data.startEndFrame.y)
                    cd.Add(data);
            }

            return cd;
        }

    }
}