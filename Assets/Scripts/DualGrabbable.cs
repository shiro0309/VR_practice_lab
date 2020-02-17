using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;

public class DualGrabbable : MonoBehaviour
    //Called on the drag object when dragging is about to begin. 
    , IColliderEventDragStartHandler
    , IColliderEventDragUpdateHandler
    , IColliderEventDragEndHandler
{
    public class Grabber
    {
        // コントローラー
        public IColliderEventCaster eventCaster { get; private set; }
        // 掴んでいるオブジェクト
        public GameObject grabbingObj { get; private set; }
        //RigidPose:TransformからScaleをなくしたもの
        // コントローラーの位置と向き
        public RigidPose grabberOrigin { get { return new RigidPose(eventCaster.transform); } }
        // コントローラーからオブジェクトまでの offset （コントローラーを親、オブジェクトを子としたときの localPosition のようなもの）
        public RigidPose grabOffset { get; private set; }
        // コントローラーの現在の位置と向き、offset から計算したオブジェクトの位置と向き
        public RigidPose grabPose { get { return grabberOrigin * grabOffset; } }

        public Grabber(IColliderEventCaster eventCaster, GameObject obj)
        {
            Debug.Log(3);
            this.eventCaster = eventCaster;
            this.grabbingObj = obj;
            this.grabOffset = RigidPose.FromToPose(new RigidPose(eventCaster.transform), new RigidPose(obj.transform));
        }
    }
    public class DualGrabber
    {

        public Grabber oneGrabber;
        public Grabber otherGrabber;

        public bool isNotGrabbed { get { return oneGrabber == null && otherGrabber == null; } }
        public bool isSoloGrabbed { get { return oneGrabber != null && otherGrabber == null; } }
        public bool isDualGrabbed { get { return oneGrabber != null && otherGrabber != null; } }

        private Transform targetTransform;

        // 両手で掴んだときの、オブジェクトのscale
        Vector3 _initScale;
        // 両手で掴んだときの、オブジェクトのrotation
        Quaternion _initRot;

        // 両手で掴んだときの、両手間の距離
        private float _initMagnitude;
        // 両手で掴んだときの、片方の手からもう片方の手への向き
        private Vector3 _initDir;

        public void UpdateTransform()
        {
            Debug.Log(3);
            // 片手で掴んでいるとき
            if (isSoloGrabbed)
            {
                targetTransform.position = oneGrabber.grabPose.pos;
                targetTransform.rotation = oneGrabber.grabPose.rot;
            }
            // 両手で掴んでいるとき
            else if (isDualGrabbed)
            {
                var currentDir = (otherGrabber.grabberOrigin.pos - oneGrabber.grabberOrigin.pos).normalized;
                var rot = Quaternion.FromToRotation(_initDir, currentDir);

                var currentMagnitude = (otherGrabber.grabberOrigin.pos - oneGrabber.grabberOrigin.pos).magnitude;
                var scale = currentMagnitude / _initMagnitude;

                targetTransform.position = (oneGrabber.grabPose.pos + otherGrabber.grabPose.pos) / 2;
                targetTransform.rotation = rot * _initRot;
                targetTransform.localScale = scale * _initScale;
            }
        }

        public void AddGrabber(Grabber grabber)
        {
            Debug.Log(3);
            if (oneGrabber == null)
            {
                oneGrabber = grabber;
                // 片手で掴み始めたときに、対象とするオブジェクトの transform を取得する
                targetTransform = grabber.grabbingObj.transform;
            }
            else
            {
                otherGrabber = grabber;

                // 両手で掴み始めたとき
                // 両手の距離、片方の手からもう片方の手までの向き
                _initMagnitude = (otherGrabber.grabberOrigin.pos - oneGrabber.grabberOrigin.pos).magnitude;
                _initDir = (otherGrabber.grabberOrigin.pos - oneGrabber.grabberOrigin.pos).normalized;

                // 両手で掴んだときのオブジェクトの scale と rotation を保存
                _initScale = targetTransform.localScale;
                _initRot = targetTransform.rotation;
            }
        }

        public void Clear()
        {
            oneGrabber = null;
            otherGrabber = null;
        }
    }

    private DualGrabber _dualGrabber = new DualGrabber();

    // Trigger を引いたとき
    public void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != ColliderButtonEventData.InputButton.Trigger) { return; }

        // 掴んでいるコントローラーと掴んでいるオブジェクトを指定して grabber を生成
        var grabber = new Grabber(eventData.eventCaster, gameObject);
        _dualGrabber.AddGrabber(grabber);
        Debug.Log(3);
    }

    // Trigger を引いている間
    public void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        _dualGrabber.UpdateTransform();
        Debug.Log(3);
    }

    // Trigger を離したとき
    public void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        _dualGrabber.Clear();
        Debug.Log(3);
    }
}