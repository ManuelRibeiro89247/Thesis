using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;
using System;

using Stopwatch = System.Diagnostics.Stopwatch;
using Mediapipe.Unity;
using Mediapipe;


public class testeholistic : MonoBehaviour
{
    [SerializeField] private TextAsset _configAsset;
    [SerializeField] private RawImage _screen;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;

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

        var stopwatch = new Stopwatch();

        _graph = new CalculatorGraph(_configAsset.text);
        var multiFaceLandmarksStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(_graph, "face_landmarks"); ////////////////////////////////////
        multiFaceLandmarksStream.StartPolling().AssertOk();


        var sidePacket = new SidePacket();

        sidePacket.Emplace("num_hands", new IntPacket(2));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(true));
        sidePacket.Emplace("input_rotation", new IntPacket(0));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(true));

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

        try
        {
            /*while (true)
            {*/
                _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
                var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
                var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
                _graph.AddPacketToInputStream("input_video", new ImageFramePacket(imageFrame, new Timestamp(currentTimestamp))).AssertOk();

                //yield return new WaitForEndOfFrame();

                if (multiFaceLandmarksStream.TryGetNext(out var facelandmarks))
                {
                    if (facelandmarks != null && facelandmarks.Count > 0)
                    {
                        foreach (var landmarks in facelandmarks)
                        {
                            // _multiFaceLandmarksAnnotationController.DrawNow(multiFaceLandmarks);
                            // top of the head
                            var topOfHead = landmarks.Landmark[10];
                            var Dot1 = landmarks.Landmark[61];
                            var Dot2 = landmarks.Landmark[146];
                            Debug.Log($"Image Coordinates: {Dot2}");
                        }
                    }
                }
                else
                {
                    // _multiFaceLandmarksAnnotationController.DrawNow(null);
                }


            //}
        } catch (System.Exception ex) 
        {
            Debug.LogError($"Error : {ex.Message}");
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



