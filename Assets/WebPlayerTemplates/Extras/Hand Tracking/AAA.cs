using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.HandTracking;
using Mediapipe;
using Mediapipe.Unity;
//using UnityEditor.SceneTemplate;

public class AAA : MonoBehaviour
{
   /* public HandTrackingGraph handTrackingGraph;

    public ImageSource imageSource;

    float time = 3.0f;

    private List<Detection> palmDetections = new List<Detection>();
    private List<NormalizedRect> handRectsFromPalmDetections = new List<NormalizedRect>();
    private List<NormalizedLandmarkList> handLandmarks = new List<NormalizedLandmarkList>();
    private List<LandmarkList> handWorldLandmarks = new List<LandmarkList>();
    private List<NormalizedRect> handRectsFromLandmarks = new List<NormalizedRect>();
    private List<ClassificationList> handedness = new List<ClassificationList>();

    private void Start()
    {
        if (time >= 0)
        {
            time -= Time.deltaTime;
        } else
        {
            // Call StartRun function from the handTrackingGraph object
            handTrackingGraph.StartRun(imageSource);
        }

    }

    private void Update()
    {
        if (time >= 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            // Call TryGetNext and receive the output variables
            if (handTrackingGraph.TryGetNext(out palmDetections, out handRectsFromPalmDetections,
                out handLandmarks, out handWorldLandmarks,
                out handRectsFromLandmarks, out handedness))
            {
                // Process the output variables
                // Example: Print the number of palm detections
                Debug.Log("Number of palm detections: " + handLandmarks);
            }
            else
            {
                Debug.Log("Error retrieving hand landmarks");
            }
        }
    }*/



}
