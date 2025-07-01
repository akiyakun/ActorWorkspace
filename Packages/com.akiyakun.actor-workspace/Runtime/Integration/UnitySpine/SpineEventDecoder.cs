using UnityEngine;

namespace ActorWorkspace.UnitySpine
{
    public class SpineEventDecoder : IAWEventDecoder
    {
        public AWEventData Decode(object rawData)
        {
            Spine.Event spineEvent = rawData as Spine.Event;
            Debug.Assert(spineEvent != null, "SpineEventDecoder: rawData is not a Spine.Event");
            if (spineEvent == null) return null;

            AWEventData eventData = new AWEventData
            {
                Name = spineEvent.Data.Name,
                Int = spineEvent.Int,
                Float = spineEvent.Float,
                // UserData = spineEvent.UserData
            };

            if (string.IsNullOrEmpty(spineEvent.Data.AudioPath) == false)
            {
                // AudioPathに値がある場合はAudioイベントとする
                eventData.EventType = AWEventType.Audio;
                eventData.String = spineEvent.Data.AudioPath;
            }
            else
            {
                // eventData.EventType = AWEventType.Unknonwn;// 初期値なので設定を省略
                eventData.String = spineEvent.String;
            }

            return eventData;
        }
    }
}
