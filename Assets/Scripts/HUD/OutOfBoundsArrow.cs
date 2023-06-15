using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutOfBoundsArrow : MonoBehaviour
{

    //[SerializeField]
    //private CanvasGroup arrow;

    //[SerializeField]
    //private WallManager wallManager;

    

    //[SerializeField]
    //private ArrowType arrowType;


    //private float fadeTime = 0.35f;

    //private bool active = false;

    //private bool lastEnter = true;
    //private Side lastSide;

    //private Coroutine coroutine;

    //private bool showArrow = false;

    //private WallInfo wallInfo;

    //void OnEnable() {
    //    wallManager.stateUpdateEvent.AddListener(OnWallUpdated);
    //    OnWallUpdated(wallManager.CreateWallInfo());
    //}

    //void OnDisable() {
    //    wallManager.stateUpdateEvent.RemoveListener(OnWallUpdated);
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (arrowType == ArrowType.PointingCenter && showArrow)
    //    {
    //        transform.right = new Vector3(wallInfo.meshCenter.x, wallInfo.meshCenter.y, arrow.transform.position.z) - arrow.transform.position;
    //    }
    //}


    //public void Reset() {
    //    if (coroutine != null) { StopCoroutine(coroutine); }
    //    coroutine = FadingUtils.FadeRoutine(handler: this, Obj: arrow.transform.gameObject, fadeTime: fadeTime, fadeDirection: FadeAction.Out);
    //    showArrow = false;
    //}

    //private void OnWallUpdated(WallInfo w) {
    //    wallInfo = w;
    //    active = wallInfo.active;
    //    if (!lastEnter && active) { ShowArrow(lastSide); }
    //}

    //public void OnMotorSpaceEnter(EnterMotorSpaceInfo m) {
    //    lastEnter = m.enter;
    //    lastSide = m.side;
    //    Debug.Log("MotorSpaceEnter " + m.enter + "active " + active);
    //    if (m.enter && active) {
    //        Debug.Log("MotorSpaceEnter " + m.side);
    //        Reset();
    //    } else if (!m.enter && active) {
    //        Debug.Log("MotorSpaceExit " + m.side);
    //        ShowArrow(m.side);
    //    } else {
    //        Reset();
    //    }
    //}

    //public void ShowArrow(Side s) {
    //    if (coroutine != null) { StopCoroutine(coroutine); }
    //    arrow.transform.rotation = Quaternion.identity;
    //    showArrow = true;
    //    transform.right = new Vector3(wallInfo.meshCenter.x,wallInfo.meshCenter.y,arrow.transform.position.z) - arrow.transform.position;
    //    // if (s == Side.Left) {
    //    //     arrow.transform.Rotate(0.0f,0.0f,180.0f);
    //    // } else if (s == Side.Right) {
    //    //     arrow.transform.rotation = Quaternion.identity;
    //    // } else if (s == Side.Top) {
    //    //     arrow.transform.Rotate(0.0f,0.0f,90.0f);
    //    // } else if (s == Side.Bottom) {
    //    //     arrow.transform.Rotate(0.0f,0.0f,270.0f);
    //    // }
    //    coroutine = FadingUtils.FadeRoutine(handler: this, Obj: arrow.transform.gameObject, fadeTime: fadeTime, fadeDirection: FadeAction.In);
    //}

    //public void ShowArrow(Side s)
    //{
    //    if (coroutine != null) { StopCoroutine(coroutine); }
    //    arrow.transform.rotation = Quaternion.identity;
    //    showArrow = true;

    //    switch (arrowType)
    //    {
    //        case ArrowType.PointingCenter:
    //            transform.right = new Vector3(wallInfo.meshCenter.x, wallInfo.meshCenter.y, arrow.transform.position.z) - arrow.transform.position;
    //            coroutine = FadingUtils.FadeRoutine(handler: this, Obj: arrow.transform.gameObject, fadeTime: fadeTime, fadeDirection: FadeAction.In);

    //            break;

    //        case ArrowType.SideAligned:
    //            arrow.transform.rotation = Quaternion.identity;
    //            if (s == Side.Left)
    //            {
    //                arrow.transform.Rotate(0.0f, 0.0f, 0.0f);  // Pointing right
    //            }
    //            else if (s == Side.Right)
    //            {
    //                arrow.transform.Rotate(0.0f, 0.0f, 180.0f);  // Pointing left
    //            }
    //            else if (s == Side.Top)
    //            {
    //                arrow.transform.Rotate(0.0f, 0.0f, 270.0f);  // Pointing bottom
    //            }
    //            else if (s == Side.Bottom)
    //            {
    //                arrow.transform.Rotate(0.0f, 0.0f, 90.0f);  // Pointing top
    //            }
    //            break;
    //    }

    //}


}
