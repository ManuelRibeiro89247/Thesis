using UnityEngine;
using Mediapipe.Unity;
using Mediapipe;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;
using System;

public class HandTrack : MonoBehaviour
{
    [SerializeField] private TextAsset _configAsset;
    [SerializeField] private RawImage _screen;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;
    [SerializeField] private MultiHandLandmarkListAnnotationController _multiHandLandmarksAnnotationController;

    //
    //float logInterval = 5f;
    //float elapseInterval = 0f;

    //

    private CalculatorGraph CalculatorGraph;
    private ResourceManager _resourceManager;

    private WebCamTexture _webCamTexture;
    private Texture2D _inputTexture;
    private Color32[] _inputPixelData;

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

        _screen.texture = _webCamTexture;

        _resourceManager = new LocalResourceManager();
        yield return _resourceManager.PrepareAssetAsync("hand_landmark_full.bytes");

        //

        /**/CalculatorGraph = new CalculatorGraph(_configAsset.text);


        // MUDAR OUTPUT STREAM PARA UM DOS DA hand_tracking_live.txt
        var poller = CalculatorGraph.AddOutputStreamPoller<string>("hand_landmarks", true).Value();

        /*if (poller.Ok())
        {
            var values = poller.Value();  
        }*/

        
        
        //
        
        var stopwatch = new Stopwatch();

        //CalculatorGraph = new CalculatorGraph(_configAsset.text);


        var handLandmarksStream = new OutputStream<NormalizedLandmarkListVectorPacket, List<NormalizedLandmarkList>>(CalculatorGraph, "hand_landmarks");
        handLandmarksStream.StartPolling().AssertOk();
        
        

        var sidePacket = new SidePacket();
        sidePacket.Emplace("num_hands", new IntPacket(2));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(false));
        sidePacket.Emplace("input_rotation", new IntPacket(0));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(true));


        CalculatorGraph.StartRun(sidePacket).AssertOk();
        stopwatch.Start();

        var screenRect = _screen.GetComponent<RectTransform>().rect;

        //
        var packet = new StringPacket();
       /* while (poller.Next(packet))
        {
            UnityEngine.Debug.Log(packet.Get());
        }*/
        //

        while (true)
        {
            _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
            var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
            var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
            CalculatorGraph.AddPacketToInputStream("input_video", new ImageFramePacket(imageFrame, new Timestamp(currentTimestamp))).AssertOk();

            yield return new WaitForEndOfFrame();

            if (handLandmarksStream.TryGetNext(out var handLandmarks))
            {
                _multiHandLandmarksAnnotationController.DrawNow(handLandmarks);

                /*elapseInterval += Time.deltaTime;
                if (elapseInterval >= logInterval)
                {
                    //UnityEngine.Debug.Log(packet.Get());
                    elapseInterval = 0f;
                }*/

                 /*if (poller.Next(packet))
                 {
                     if (!packet.IsEmpty()) 
                     {
                         var value = packet.Get();
                         //UnityEngine.Debug.Log(value);
                     }
                 }*/

  

            }
            else
            {
                _multiHandLandmarksAnnotationController.DrawNow(null);
            }
        }
    }

    private void OnDestroy()
    {
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
        }

        if (CalculatorGraph != null)
        {
            try
            {
                CalculatorGraph.CloseInputStream("input_video").AssertOk();
                CalculatorGraph.WaitUntilDone().AssertOk();
            }
            finally
            {

                CalculatorGraph.Dispose();
            }
        }
    }
}

