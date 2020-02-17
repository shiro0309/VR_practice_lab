using UnityEngine;
using Pose = HTC.UnityPlugin.PoseTracker.Pose;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.PoseTracker;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Eri_First : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
    , IPointerClickHandler
{
    private HashSet<PointerEventData> hovers = new HashSet<PointerEventData>();
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hovers.Add(eventData) && hovers.Count == 1)
        {
            Debug.Log("turn to highlight state");
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (hovers.Remove(eventData) && hovers.Count == 0)
        {
            Debug.Log("turn to normal state");
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.IsViveButton(ControllerButton.Trigger))
        {
            Debug.Log("Vive button triggered!");
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Standalone button triggered!");
        }
    }
}