using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;

using Stopwatch = System.Diagnostics.Stopwatch;
using Mediapipe.Unity;
using Mediapipe;

public class Triple : MonoBehaviour
{
    [SerializeField] private TextAsset _configAsset;
    [SerializeField] private RawImage _screen;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;
    [SerializeField] private MultiFaceLandmarkListAnnotationController _multiFaceLandmarksAnnotationController;
    [SerializeField] private MultiHandLandmarkListAnnotationController _multiHandLandmarksAnnotationController;
    [SerializeField] private PoseLandmarkListAnnotationController _poseLandmarksAnnotationController;

    private CalculatorGraph _graph;
    private ResourceManager _resourceManager;

    private WebCamTexture _webCamTexture;
    private Texture2D _inputTexture;
    private Color32[] _inputPixelData;
    private Texture2D _outputTexture;
    private Color32[] _outputPixelData;

    private IEnumerator Start()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            throw new System.Exception("Web Camera devices are not found");
        }
        var webCamDevice = WebCamTexture.devices[0];
        _webCamTexture = new WebCamTexture(webCamDevice.name, _width, _height, _fps);
        _webCamTexture.Play();

        yield return new WaitUntil(() => _webCamTexture.width > 16);

        _screen.rectTransform.sizeDelta = new Vector2(_width, _height);

        _inputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        _inputPixelData = new Color32[_width * _height];
        _outputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        _outputPixelData = new Color32[_width * _height];

        _screen.texture = _outputTexture;

        _resourceManager = new LocalResourceManager();
        yield return _resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
        yield return _resourceManager.PrepareAssetAsync("face_landmark_with_attention.bytes");
        yield return _resourceManager.PrepareAssetAsync("hand_landmark_full.bytes");
        yield return _resourceManager.PrepareAssetAsync("pose_landmark_full.bytes");

        var stopwatch = new Stopwatch();

        _graph = new CalculatorGraph(_configAsset.text);
        
        var outputVideoStream = new OutputStream<ImageFramePacket, ImageFrame>(_graph, "output_video");

        var handsStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "hand_landmarks");
        var handednessStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "handedness");
        var multiFaceLandmarksStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
        var PoseStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "pose_landmarks");

        outputVideoStream.StartPolling().AssertOk();

        handsStream.StartPolling().AssertOk();
        handednessStream.StartPolling().AssertOk();
        multiFaceLandmarksStream.StartPolling().AssertOk();
        PoseStream.StartPolling().AssertOk();


        var sidePacket = new SidePacket();

        sidePacket.Emplace("num_hands", new IntPacket(2));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(true));
        sidePacket.Emplace("input_rotation", new IntPacket(0));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(true));

        sidePacket.Emplace("input_rotation", new IntPacket(0));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(true));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(true));


        //sidePacket.Emplace("model_complexity", new IntPacket(1));
        sidePacket.Emplace("smooth_landmarks", new BoolPacket(true));
        sidePacket.Emplace("enable_segmentation", new BoolPacket(true));
        sidePacket.Emplace("smooth_segmentation", new BoolPacket(true));


        sidePacket.Emplace("output_rotation", new IntPacket(0));
        sidePacket.Emplace("output_horizontally_flipped", new BoolPacket(true));
        sidePacket.Emplace("output_vertically_flipped", new BoolPacket(true));


        _graph.StartRun(sidePacket).AssertOk();

        stopwatch.Start();

        var screenRect = _screen.GetComponent<RectTransform>().rect;

        while (true)
        {
            _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
            var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
            var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
            _graph.AddPacketToInputStream("input_video", new ImageFramePacket(imageFrame, new Timestamp(currentTimestamp))).AssertOk();

            yield return new WaitForEndOfFrame();


            /*if (outputVideoStream.TryGetNext(out var outputVideo))
            {
                if (outputVideo.TryReadPixelData(_outputPixelData))
                {
                    _outputTexture.SetPixels32(_outputPixelData);
                    _outputTexture.Apply();
                }
            }*/


            if (multiFaceLandmarksStream.TryGetNext(out var multiFaceLandmarks))
            {
                if (multiFaceLandmarks != null && multiFaceLandmarks.Count > 0)
                {
                    foreach (var landmarks in multiFaceLandmarks)
                    {
                        _multiFaceLandmarksAnnotationController.DrawNow(multiFaceLandmarks);
                        // top of the head
                        var topOfHead = landmarks.Landmark[10];
                        var Dot1 = landmarks.Landmark[61];
                        var Dot2 = landmarks.Landmark[146];
                        Debug.Log($"Unity Local Coordinates: {screenRect.GetPoint(Dot2)}, Image Coordinates: {Dot2}");
                    }
                }
            }
            else
            {
                _multiFaceLandmarksAnnotationController.DrawNow(null);
            }

            /*if (handednessStream.TryGetNext(out var hands))
            {
                if (hands != null && hands.Count > 0)
                {
                    foreach (var landmarks in hands)
                    {
                        var ruan = landmarks.Landmark[1];
                        
                        Debug.Log(ruan);
                    }
                }
            }*/
            




           if (handsStream.TryGetNext(out var handLandmarks))
            {
                if (handLandmarks != null && handLandmarks.Count > 0)
                {
                    foreach (var landmarks in handLandmarks)
                    {
                        _multiHandLandmarksAnnotationController.DrawNow(handLandmarks);
                        // top of the head
                        //var topOfHead = landmarks.Landmark[10];
                        //var Dot1 = landmarks.Landmark[61];
                        //var Dot2 = landmarks.Landmark[146];
                        //Debug.Log($"Unity Local Coordinates: {screenRect.GetPoint(Dot2)}, Image Coordinates: {Dot2}");
                    }
                }
            }
            else
            {
                _multiHandLandmarksAnnotationController.DrawNow(null);
            } 

           /* if (PoseStream.TryGetNext(out var poseLandmarks))
            {
                if (poseLandmarks != null && poseLandmarks.Count > 0)
                {
                    foreach (var landmarks in poseLandmarks)
                    {
                        _poseLandmarksAnnotationController.DrawNowNormalizedLandmarkList(landmarks);
                    }
                }

            }
            else
            {
                _poseLandmarksAnnotationController.DrawNowNormalizedLandmarkList(null);
            }*/



        }
    }

    private void OnDestroy()
    {
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
        }

        if (_graph != null)
        {
            try
            {
                _graph.CloseInputStream("input_video").AssertOk();
                _graph.WaitUntilDone().AssertOk();
            }
            finally
            {

                _graph.Dispose();
            }
        }
    }
}
