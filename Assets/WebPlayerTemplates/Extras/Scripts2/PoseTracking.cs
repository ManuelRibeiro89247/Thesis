using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;

using Stopwatch = System.Diagnostics.Stopwatch;
using Mediapipe.Unity;
using Mediapipe;


public class PoseTracking : MonoBehaviour
{
    [SerializeField] private TextAsset _configAsset;
    [SerializeField] private RawImage _screen;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;
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
        yield return _resourceManager.PrepareAssetAsync("pose_landmark_full.bytes");

        

        var stopwatch = new Stopwatch();

        _graph = new CalculatorGraph(_configAsset.text);
        //var outputVideoStream = new OutputStream<ImageFramePacket, ImageFrame>(_graph, "output_video");
        var PoseStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "pose_landmarks");
        //var multiFaceLandmarksStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
        //outputVideoStream.StartPolling().AssertOk();
        PoseStream.StartPolling().AssertOk();
        //multiFaceLandmarksStream.StartPolling().AssertOk();
        var sidePacket = new SidePacket();

        sidePacket.Emplace("input_rotation", new IntPacket(0));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(true));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(true));


        sidePacket.Emplace("model_complexity", new IntPacket(1));
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

            
                if (PoseStream.TryGetNext(out var poseLandmarks))
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
            }



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
