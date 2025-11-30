using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    // SkeletonAnimation用
    public class SpineAnimationEventDecoder : IAWAnimationEventDecoder<Spine.Event>
    {
        public AWAnimationEventData Decode(Spine.Event rawData)
        {
            // Debug.Assert(spineEvent != null, "SpineEventDecoder: rawData is not a Spine.Event");
            // if (spineEvent == null) return null;

            AWAnimationEventData eventData = new AWAnimationEventData
            {
                Name = rawData.Data.Name,
                Int = rawData.Int,
                Float = rawData.Float,
                // UserData = spineEvent.UserData
            };

            if (string.IsNullOrEmpty(rawData.Data.AudioPath) == false)
            {
                // AudioPathに値がある場合はAudioイベントとする
                // eventData.EventType = AWEventType.Audio;
                eventData.String = rawData.Data.AudioPath;
            }
            else
            {
                // eventData.EventType = AWEventType.Unknonwn;// 初期値なので設定を省略
                eventData.String = rawData.String;
            }

            return eventData;
        }
    }
}
