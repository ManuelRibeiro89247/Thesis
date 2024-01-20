using Mediapipe.Unity;
using Mediapipe;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using System.Text;

public class TesteHolistic : MonoBehaviour
{
    [SerializeField] private PoseLandmarkListAnnotation _poseLandmarkListAnnotation;
    [SerializeField] private FaceLandmarkListWithIrisAnnotation _faceLandmarkListWithIrisAnnotation;
    [SerializeField] private HandLandmarkListAnnotation _lefthandLandmarkListAnnotation;
    [SerializeField] private HandLandmarkListAnnotation _righthandLandmarkListAnnotation;

    [SerializeField] private TextAsset _configAssetCpu;


    [SerializeField] private TextAsset _configAssetGpu;


    [SerializeField] private RawImage _screen;
    [SerializeField] private RawImage _screen2;

    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;

    /* */ public TextMeshProUGUI TextResult;
    /* */public TextMeshProUGUI Result;

    private CalculatorGraph _graph;

    private WebCamTexture _webCamTexture;
    private Texture2D _inputTexture;
    private Color32[] _inputPixelData;

    private string previousValue = "";
    private bool canSendRequest = true;
    private float lastRequestTime;
    private string combinedValue = "";
    private float clearInterval = 10f;
    private float lastClearTime = 0f;
    private StringBuilder combinedValueBuilder = new StringBuilder();

    List<float> valuesList = new List<float>();
    List<float> valuesTest = new List<float>();
    List<float> valuesTest1 = new List<float>();
    List<float> valuesTest2 = new List<float>();
    List<float> valuesTest3 = new List<float>();
    List<float> valuesTest4 = new List<float>();
    List<float> valuesTest5 = new List<float>();
    List<float> valuesTest6 = new List<float>();
    List<float> valuesTest7 = new List<float>();
    List<float> valuesTest8 = new List<float>();

    //List<List<float>> arrayOfLists = new List<List<float>>();

    public GameObject screenDisplay;
    public GameObject screenDisplay2;
    public NetworkManager networkManagerObject;
    private NetworkManager networkManager;

    //public ProxySocket proxySocketObject;
    //private ProxySocket proxySocket;

    int valuesTestCount = 1;
    int counter = 0;
    string json = null;

    //List<float> valuesPose = new List<float>();
    //List<float> valuesLeft = new List<float>();
    //List<float> valuesRight = new List<float>();
    //List<float> valuesFace = new List<float>();
    //List<Vector2> poseLandmarkValuesList = new List<Vector2>();
    //private string serverUrl = " https://cloud1cvig.ccg.pt:16301/mp_estimator";
    // Start is called before the first frame update

    [System.Serializable]
    public class FloatListWrapper
    {
        public List<float> floatList;

        public FloatListWrapper(List<float> list)
        {
            floatList = list;
        }
    }

    [System.Serializable]
    public class LandmarkItem
    {
        public List<float> pose_landmarks;
        public List<float> left_hand_landmarks;
        public List<float> right_hand_landmarks;
        public List<float> face_landmarks;
    }

    [System.Serializable]
    public class SpeedConfig
    {
        public int lento;
        public int medio;
        public int rapido;
    }


    [System.Serializable]
    public class Final
    {
        public SpeedConfig speed;
        public LandmarkItem mp;
        public List<string> mp_detection_mandatory;
    }


    

    [System.Serializable]
    public class VocabModelOpts
    {
        public string[] Final;
        public string[] Compras;
    }

    [System.Serializable]
    public class Landmarks
    {
        public List<float> pose_landmarks;
        public List<float> left_hand_landmarks;
        public List<float> right_hand_landmarks;
        public List<float> face_landmarks;
    }

    [System.Serializable]
    public class CollectNumber
    {
        public int lento;
        public int medio;
        public int rapido;
    }

    [System.Serializable]
    public class PostRequestData
    {
        public VocabModelOpts vocab_model_opts;
        public string vocab_selected;
        public string model_selected;
        public Landmarks landmarks;
        public CollectNumber collectnumber;
    }

    [System.Serializable]
    public class NestedFloatList
    {
        public List<float> valuesTest1;
        public List<float> valuesTest2;
        public List<float> valuesTest3;
        public List<float> valuesTest4;
        public List<float> valuesTest5;
        public List<float> valuesTest6;
        public List<float> valuesTest7;
        public List<float> valuesTest8;
    }



    IEnumerator Start()
    {



        // _poseLandmarkListAnnotation.rotationAngle = RotationAngle.Rotation0;
        //_poseLandmarkListAnnotation.isMirrored = false;
        TextResult.text = "starting";

        if (WebCamTexture.devices.Length == 0)
        {
            throw new System.Exception("Web Camera devices are not found");
            //TextResult.text = "devices.Length == 0";
        }
        var webCamDevice = WebCamTexture.devices[0];
        _webCamTexture = new WebCamTexture(webCamDevice.name, _width, _height, _fps);
        _webCamTexture.Play();
        yield return new WaitUntil(() => _webCamTexture.width > 16);
        TextResult.text = "_webCamTexture.width > 16";





        _screen.rectTransform.sizeDelta = new Vector2(_width, _height);

        _inputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        _inputPixelData = new Color32[_width * _height];


        _screen.texture = _webCamTexture;
        _screen2.texture = _webCamTexture;

        TextResult.text = "_screen.texture = _webCamTexture";

        /**/

        var resourceManager = new StreamingAssetsResourceManager();
        TextResult.text = "after manager";
        yield return resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
        TextResult.text = "after yield return resmanager";
        yield return resourceManager.PrepareAssetAsync("face_landmark.bytes");
        yield return resourceManager.PrepareAssetAsync("hand_landmark_full.bytes");
        yield return resourceManager.PrepareAssetAsync("hand_recrop.bytes");
        yield return resourceManager.PrepareAssetAsync("handedness.txt");
        yield return resourceManager.PrepareAssetAsync("pose_detection.bytes");
        yield return resourceManager.PrepareAssetAsync("pose_landmark_full.bytes");
        TextResult.text = "after return bytes";
        var stopwatch = new Stopwatch();
        var sidePacket = new SidePacket();


        //_screen.uvRect = new UnityEngine.Rect(1, 0, -1, 1);

        _screen.uvRect = new UnityEngine.Rect(1,1,640,480);
        sidePacket.Emplace("model_complexity", new IntPacket(1));
        sidePacket.Emplace("smooth_landmarks", new BoolPacket(true));
        sidePacket.Emplace("input_rotation", new IntPacket(180));
        sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(true));
        sidePacket.Emplace("input_vertically_flipped", new BoolPacket(false));
        sidePacket.Emplace("output_rotation", new IntPacket(0));
        sidePacket.Emplace("output_vertically_flipped", new BoolPacket(false));
        sidePacket.Emplace("output_horizontally_flipped", new BoolPacket(true));
        TextResult.text = "before graph";
        _graph = new CalculatorGraph(_configAssetCpu.text);



        var poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(_graph, "pose_landmarks", true);
        var faceLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(_graph, "face_landmarks", true);
        var lefthandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(_graph, "left_hand_landmarks", true);
        var righthandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(_graph, "right_hand_landmarks", true);
        TextResult.text = "before polling";
        poseLandmarksStream.StartPolling().AssertOk();
        faceLandmarksStream.StartPolling().AssertOk();
        lefthandLandmarksStream.StartPolling().AssertOk();
        righthandLandmarksStream.StartPolling().AssertOk();
        _graph.StartRun(sidePacket).AssertOk();
        TextResult.text = "before stopwatch.Start()";
        stopwatch.Start();

        TextResult.text = "stopwatch.Start()";



        while (true)
        {

            _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
            var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
            var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
            _graph.AddPacketToInputStream("input_video", new ImageFramePacket(imageFrame, new Timestamp(currentTimestamp))).AssertOk();
            yield return new WaitForEndOfFrame();

            if (poseLandmarksStream.TryGetNext(out var poseLandmarks))
            {
                if (poseLandmarks != null)
                {
                    if (poseLandmarks.Landmark != null && poseLandmarks.Landmark.Count > 0)
                    {
                        if (screenDisplay.activeSelf)
                        {
                            _poseLandmarkListAnnotation.Draw(poseLandmarks.Landmark, PoseLandmarkListAnnotation.BodyParts.All);
                        }

                        
                        
                        var pose5 = poseLandmarks.Landmark[5];
                        var pose2 = poseLandmarks.Landmark[2];
                        var pose12 = poseLandmarks.Landmark[12];
                        var pose11 = poseLandmarks.Landmark[11];
                        var pose14 = poseLandmarks.Landmark[14];
                        var pose13 = poseLandmarks.Landmark[13];
                        
                        
                       // Debug.Log($"Pose: {pose5}");
                        
                        valuesList.Add(pose5 != null ? pose5.X : 0);
                        valuesList.Add(pose5 != null ? pose5.Y : 0);

                        valuesList.Add(pose2 != null ? pose2.X : 0);
                        valuesList.Add(pose2 != null ? pose2.Y : 0);

                        valuesList.Add(pose12 != null ? pose12.X : 0);
                        valuesList.Add(pose12 != null ? pose12.Y : 0);

                        valuesList.Add(pose11 != null ? pose11.X : 0);
                        valuesList.Add(pose11 != null ? pose11.Y : 0);

                        valuesList.Add(pose14 != null ? pose14.X : 0);
                        valuesList.Add(pose14 != null ? pose14.Y : 0);

                        valuesList.Add(pose13 != null ? pose13.X : 0);
                        valuesList.Add(pose13 != null ? pose13.Y : 0);

                        //                       
                        
                        valuesTest.Add(pose5 != null ? pose5.X : 0);
                        valuesTest.Add(pose5 != null ? pose5.X : 0);

                        valuesTest.Add(pose2 != null ? pose2.X : 0);
                        valuesTest.Add(pose2 != null ? pose2.Y : 0);

                        valuesTest.Add(pose12 != null ? pose12.X : 0);
                        valuesTest.Add(pose12 != null ? pose12.Y : 0);

                        valuesTest.Add(pose11 != null ? pose11.X : 0);
                        valuesTest.Add(pose11 != null ? pose11.Y : 0);

                        valuesTest.Add(pose14 != null ? pose14.X : 0);
                        valuesTest.Add(pose14 != null ? pose14.Y : 0);

                        valuesTest.Add(pose13 != null ? pose13.X : 0);
                        valuesTest.Add(pose13 != null ? pose13.Y : 0);


                        //Vector2 poseLandmarkValues = new Vector2(Dot2.x, Dot2.y);
                        //poseLandmarkValuesList.Add(poseLandmarkValues);
                    }
                }
            }


            

            if (lefthandLandmarksStream.TryGetNext(out var lefthandLandmarks))
            {
                if (lefthandLandmarks != null)
                {
                    if (lefthandLandmarks.Landmark != null && lefthandLandmarks.Landmark.Count > 0)
                    {
                        // _handLandmarkListAnnotation.Draw(lefthandLandmarks.Landmark, HandLandmarkListAnnotation.Hand.Left);
                        if (screenDisplay.activeSelf)
                        {
                            _lefthandLandmarkListAnnotation.Draw(lefthandLandmarks.Landmark, false);
                        }
                            
                        
                        

                        var left0 = lefthandLandmarks.Landmark[0];
                        var left1 = lefthandLandmarks.Landmark[1];
                        var left2 = lefthandLandmarks.Landmark[2];
                        var left4 = lefthandLandmarks.Landmark[4];
                        var left5 = lefthandLandmarks.Landmark[5];
                        var left8 = lefthandLandmarks.Landmark[8];
                        var left9 = lefthandLandmarks.Landmark[9];
                        var left12 = lefthandLandmarks.Landmark[12];
                        var left13 = lefthandLandmarks.Landmark[13];
                        var left16 = lefthandLandmarks.Landmark[16];
                        var left17 = lefthandLandmarks.Landmark[17];
                        var left20 = lefthandLandmarks.Landmark[20];

                        var left3 = lefthandLandmarks.Landmark[3];
                        var left6 = lefthandLandmarks.Landmark[6];
                        var left7 = lefthandLandmarks.Landmark[7];
                        var left10 = lefthandLandmarks.Landmark[10];
                        var left11 = lefthandLandmarks.Landmark[11];
                        var left14 = lefthandLandmarks.Landmark[14];
                        var left15 = lefthandLandmarks.Landmark[15];
                        var left18 = lefthandLandmarks.Landmark[18];
                        var left19 = lefthandLandmarks.Landmark[19];

                        // Debug.Log($"lefthand: {left0}");
                        // Debug.Log($"lefthand: {left1}");
                        // Debug.Log($"lefthand: {left2}");


                        valuesList.Add(left0 != null ? left0.X : 0);
                        valuesList.Add(left0 != null ? left0.Y : 0);

                        valuesList.Add(left1 != null ? left1.X : 0);
                        valuesList.Add(left1 != null ? left1.Y : 0);

                        valuesList.Add(left2 != null ? left2.X : 0);
                        valuesList.Add(left2 != null ? left2.Y : 0);

                        valuesList.Add(left4 != null ? left4.X : 0);
                        valuesList.Add(left4 != null ? left4.Y : 0);

                        valuesList.Add(left5 != null ? left5.X : 0);
                        valuesList.Add(left5 != null ? left5.Y : 0);

                        valuesList.Add(left8 != null ? left8.X : 0);
                        valuesList.Add(left8 != null ? left8.Y : 0);

                        valuesList.Add(left9 != null ? left9.X : 0);
                        valuesList.Add(left9 != null ? left9.Y : 0);

                        valuesList.Add(left12 != null ? left12.X : 0);
                        valuesList.Add(left12 != null ? left12.Y : 0);

                        valuesList.Add(left13 != null ? left13.X : 0);
                        valuesList.Add(left13 != null ? left13.Y : 0);

                        valuesList.Add(left16 != null ? left16.X : 0);
                        valuesList.Add(left16 != null ? left16.Y : 0);

                        valuesList.Add(left17 != null ? left17.X : 0);
                        valuesList.Add(left17 != null ? left17.Y : 0);

                        valuesList.Add(left20 != null ? left20.X : 0);
                        valuesList.Add(left20 != null ? left20.Y : 0);

                        //

                        valuesTest.Add(left0 != null ? left0.X : 0);
                        valuesTest.Add(left0 != null ? left0.Y : 0);

                        valuesTest.Add(left1 != null ? left1.X : 0);
                        valuesTest.Add(left1 != null ? left1.Y : 0);

                        valuesTest.Add(left2 != null ? left2.X : 0);
                        valuesTest.Add(left2 != null ? left2.Y : 0);

                        valuesTest.Add(left3 != null ? left3.X : 0);
                        valuesTest.Add(left3 != null ? left3.Y : 0);

                        valuesTest.Add(left4 != null ? left4.X : 0);
                        valuesTest.Add(left4 != null ? left4.Y : 0);

                        valuesTest.Add(left5 != null ? left5.X : 0);
                        valuesTest.Add(left5 != null ? left5.Y : 0);

                        valuesTest.Add(left6 != null ? left6.X : 0);
                        valuesTest.Add(left6 != null ? left6.Y : 0);

                        valuesTest.Add(left7 != null ? left7.X : 0);
                        valuesTest.Add(left7 != null ? left7.Y : 0);

                        valuesTest.Add(left8 != null ? left8.X : 0);
                        valuesTest.Add(left8 != null ? left8.Y : 0);

                        valuesTest.Add(left9 != null ? left9.X : 0);
                        valuesTest.Add(left9 != null ? left9.Y : 0);

                        valuesTest.Add(left10 != null ? left10.X : 0);
                        valuesTest.Add(left10 != null ? left10.Y : 0);

                        valuesTest.Add(left11 != null ? left11.X : 0);
                        valuesTest.Add(left11 != null ? left11.Y : 0);

                        valuesTest.Add(left12 != null ? left12.X : 0);
                        valuesTest.Add(left12 != null ? left12.Y : 0);

                        valuesTest.Add(left13 != null ? left13.X : 0);
                        valuesTest.Add(left13 != null ? left13.Y : 0);

                        valuesTest.Add(left14 != null ? left0.X : 0);
                        valuesTest.Add(left14 != null ? left0.Y : 0);

                        valuesTest.Add(left15 != null ? left15.X : 0);
                        valuesTest.Add(left15 != null ? left15.Y : 0);

                        valuesTest.Add(left16 != null ? left16.X : 0);
                        valuesTest.Add(left16 != null ? left16.Y : 0);

                        valuesTest.Add(left17 != null ? left17.X : 0);
                        valuesTest.Add(left17 != null ? left17.Y : 0);

                        valuesTest.Add(left18 != null ? left18.X : 0);
                        valuesTest.Add(left18 != null ? left18.Y : 0);

                        valuesTest.Add(left19 != null ? left19.X : 0);
                        valuesTest.Add(left19 != null ? left19.Y : 0);

                        valuesTest.Add(left20 != null ? left20.X : 0);
                        valuesTest.Add(left20 != null ? left20.Y : 0);

                    }
                }
            }

            if (righthandLandmarksStream.TryGetNext(out var righthandLandmarks))
            {
                if (righthandLandmarks != null)
                {
                    if (righthandLandmarks.Landmark != null && righthandLandmarks.Landmark.Count > 0)
                    {
                        // _handLandmarkListAnnotation.Draw(righthandLandmarks.Landmark, HandLandmarkListAnnotation.Hand.Right);
                        if (screenDisplay.activeSelf)
                        {
                            _righthandLandmarkListAnnotation.Draw(righthandLandmarks.Landmark, false);
                        }


                        
                       
                        var right0 = righthandLandmarks.Landmark[0];
                        var right1 = righthandLandmarks.Landmark[1];
                        var right2 = righthandLandmarks.Landmark[2];
                        var right4 = righthandLandmarks.Landmark[4];
                        var right5 = righthandLandmarks.Landmark[5];
                        var right8 = righthandLandmarks.Landmark[8];
                        var right9 = righthandLandmarks.Landmark[9];
                        var right12 = righthandLandmarks.Landmark[12];
                        var right13 = righthandLandmarks.Landmark[13];
                        var right16 = righthandLandmarks.Landmark[16];
                        var right17 = righthandLandmarks.Landmark[17];
                        var right20 = righthandLandmarks.Landmark[20];

                        var right3 = righthandLandmarks.Landmark[3];
                        var right6 = righthandLandmarks.Landmark[6];
                        var right7 = righthandLandmarks.Landmark[7];
                        var right10 = righthandLandmarks.Landmark[10];
                        var right11 = righthandLandmarks.Landmark[11];
                        var right14 = righthandLandmarks.Landmark[14];
                        var right15 = righthandLandmarks.Landmark[15];
                        var right18 = righthandLandmarks.Landmark[18];
                        var right19 = righthandLandmarks.Landmark[19];

                        // Debug.Log($"lefthand: {right0}");
                        // Debug.Log($"lefthand: {right1}");
                        // Debug.Log($"lefthand: {right2}");


                        valuesList.Add(right0 != null ? right0.X : 0);
                        valuesList.Add(right0 != null ? right0.Y : 0);

                        valuesList.Add(right1 != null ? right1.X : 0);
                        valuesList.Add(right1 != null ? right1.Y : 0);

                        valuesList.Add(right2 != null ? right2.X : 0);
                        valuesList.Add(right2 != null ? right2.Y : 0);

                        valuesList.Add(right4 != null ? right4.X : 0);
                        valuesList.Add(right4 != null ? right4.Y : 0);

                        valuesList.Add(right5 != null ? right5.X : 0);
                        valuesList.Add(right5 != null ? right5.Y : 0);

                        valuesList.Add(right8 != null ? right8.X : 0);
                        valuesList.Add(right8 != null ? right8.Y : 0);

                        valuesList.Add(right9 != null ? right9.X : 0);
                        valuesList.Add(right9 != null ? right9.Y : 0);

                        valuesList.Add(right12 != null ? right12.X : 0);
                        valuesList.Add(right12 != null ? right12.Y : 0);

                        valuesList.Add(right13 != null ? right13.X : 0);
                        valuesList.Add(right13 != null ? right13.Y : 0);

                        valuesList.Add(right16 != null ? right16.X : 0);
                        valuesList.Add(right16 != null ? right16.Y : 0);

                        valuesList.Add(right17 != null ? right17.X : 0);
                        valuesList.Add(right17 != null ? right17.Y : 0);

                        valuesList.Add(right20 != null ? right20.X : 0);
                        valuesList.Add(right20 != null ? right20.Y : 0);

                        //

                        valuesTest.Add(right0 != null ? right0.X : 0);
                        valuesTest.Add(right0 != null ? right0.Y : 0);

                        valuesTest.Add(right1 != null ? right1.X : 0);
                        valuesTest.Add(right1 != null ? right1.Y : 0);

                        valuesTest.Add(right2 != null ? right2.X : 0);
                        valuesTest.Add(right2 != null ? right2.Y : 0);

                        valuesTest.Add(right3 != null ? right3.X : 0);
                        valuesTest.Add(right3 != null ? right3.Y : 0);

                        valuesTest.Add(right4 != null ? right4.X : 0);
                        valuesTest.Add(right4 != null ? right4.Y : 0);

                        valuesTest.Add(right5 != null ? right5.X : 0);
                        valuesTest.Add(right5 != null ? right5.Y : 0);

                        valuesTest.Add(right6 != null ? right6.X : 0);
                        valuesTest.Add(right6 != null ? right6.Y : 0);

                        valuesTest.Add(right7 != null ? right7.X : 0);
                        valuesTest.Add(right7 != null ? right7.Y : 0);

                        valuesTest.Add(right8 != null ? right8.X : 0);
                        valuesTest.Add(right8 != null ? right8.Y : 0);

                        valuesTest.Add(right9 != null ? right9.X : 0);
                        valuesTest.Add(right9 != null ? right9.Y : 0);

                        valuesTest.Add(right10 != null ? right10.X : 0);
                        valuesTest.Add(right10 != null ? right10.Y : 0);

                        valuesTest.Add(right11 != null ? right11.X : 0);
                        valuesTest.Add(right11 != null ? right11.Y : 0);

                        valuesTest.Add(right12 != null ? right12.X : 0);
                        valuesTest.Add(right12 != null ? right12.Y : 0);

                        valuesTest.Add(right13 != null ? right13.X : 0);
                        valuesTest.Add(right13 != null ? right13.Y : 0);

                        valuesTest.Add(right14 != null ? right14.X : 0);
                        valuesTest.Add(right14 != null ? right14.Y : 0);

                        valuesTest.Add(right15 != null ? right15.X : 0);
                        valuesTest.Add(right15 != null ? right15.Y : 0);

                        valuesTest.Add(right16 != null ? right16.X : 0);
                        valuesTest.Add(right16 != null ? right16.Y : 0);

                        valuesTest.Add(right17 != null ? right17.X : 0);
                        valuesTest.Add(right17 != null ? right17.Y : 0);

                        valuesTest.Add(right18 != null ? right18.X : 0);
                        valuesTest.Add(right18 != null ? right18.Y : 0);

                        valuesTest.Add(right19 != null ? right19.X : 0);
                        valuesTest.Add(right19 != null ? right19.Y : 0);

                        valuesTest.Add(right20 != null ? right20.X : 0);
                        valuesTest.Add(right20 != null ? right20.Y : 0);
                    }
                }
            }

            if (faceLandmarksStream.TryGetNext(out var multiFaceLandmarks))
            {
                if (multiFaceLandmarks != null)
                {
                    if (multiFaceLandmarks.Landmark != null && multiFaceLandmarks.Landmark.Count > 0)
                    {
                        if (screenDisplay.activeSelf)
                        {
                            _faceLandmarkListWithIrisAnnotation.Draw(multiFaceLandmarks.Landmark, false, 128);

                        }

                        

                        var face61 = multiFaceLandmarks.Landmark[61];
                        var face146 = multiFaceLandmarks.Landmark[146];
                        var face91 = multiFaceLandmarks.Landmark[91];
                        var face181 = multiFaceLandmarks.Landmark[181];
                        var face84 = multiFaceLandmarks.Landmark[84];
                        var face17 = multiFaceLandmarks.Landmark[17];
                        var face314 = multiFaceLandmarks.Landmark[314];
                        var face405 = multiFaceLandmarks.Landmark[405];
                        var face321 = multiFaceLandmarks.Landmark[321];
                        var face375 = multiFaceLandmarks.Landmark[375];
                        var face291 = multiFaceLandmarks.Landmark[291];
                        var face308 = multiFaceLandmarks.Landmark[308];
                        var face324 = multiFaceLandmarks.Landmark[324];
                        var face318 = multiFaceLandmarks.Landmark[318];
                        var face402 = multiFaceLandmarks.Landmark[402];
                        var face317 = multiFaceLandmarks.Landmark[317];
                        var face14 = multiFaceLandmarks.Landmark[14];
                        var face87 = multiFaceLandmarks.Landmark[87];
                        var face178 = multiFaceLandmarks.Landmark[178];
                        var face88 = multiFaceLandmarks.Landmark[88];
                        var face95 = multiFaceLandmarks.Landmark[95];
                        var face185 = multiFaceLandmarks.Landmark[185];
                        var face40 = multiFaceLandmarks.Landmark[40];
                        var face39 = multiFaceLandmarks.Landmark[39];
                        var face37 = multiFaceLandmarks.Landmark[37];
                        var face0 = multiFaceLandmarks.Landmark[0];
                        var face267 = multiFaceLandmarks.Landmark[267];
                        var face269 = multiFaceLandmarks.Landmark[269];
                        var face270 = multiFaceLandmarks.Landmark[270];
                        var face409 = multiFaceLandmarks.Landmark[409];
                        var face415 = multiFaceLandmarks.Landmark[415];
                        var face310 = multiFaceLandmarks.Landmark[310];
                        var face311 = multiFaceLandmarks.Landmark[311];
                        var face312 = multiFaceLandmarks.Landmark[312];
                        var face13 = multiFaceLandmarks.Landmark[13];
                        var face82 = multiFaceLandmarks.Landmark[82];
                        var face81 = multiFaceLandmarks.Landmark[81];
                        var face42 = multiFaceLandmarks.Landmark[42];
                        var face183 = multiFaceLandmarks.Landmark[183];
                        var face78 = multiFaceLandmarks.Landmark[78];


                       // Debug.Log($"Face: {face61}");
                         
                         
                         valuesList.Add(face61 != null ? face61.X : 0);
                         valuesList.Add(face61 != null ? face61.Y : 0);
                         
                         valuesList.Add(face146 != null ? face146.X : 0);
                         valuesList.Add(face146 != null ? face146.Y : 0);

                         valuesList.Add(face91 != null ? face91.X : 0);
                         valuesList.Add(face91 != null ? face91.Y : 0);

                         valuesList.Add(face181 != null ? face181.X : 0);
                         valuesList.Add(face181 != null ? face181.Y : 0);

                         valuesList.Add(face84 != null ? face84.X : 0);
                         valuesList.Add(face84 != null ? face84.Y : 0);

                         valuesList.Add(face17 != null ? face17.X : 0);
                         valuesList.Add(face17 != null ? face17.Y : 0);

                         valuesList.Add(face314 != null ? face314.X : 0);
                         valuesList.Add(face314 != null ? face314.Y : 0);

                         valuesList.Add(face405 != null ? face405.X : 0);
                         valuesList.Add(face405 != null ? face405.Y : 0);

                         valuesList.Add(face321 != null ? face321.X : 0);
                         valuesList.Add(face321 != null ? face321.Y : 0);

                         valuesList.Add(face375 != null ? face375.X : 0);
                         valuesList.Add(face375 != null ? face375.Y : 0);

                         valuesList.Add(face291 != null ? face291.X : 0);
                         valuesList.Add(face291 != null ? face291.Y : 0);

                         valuesList.Add(face308 != null ? face308.X : 0);
                         valuesList.Add(face308 != null ? face308.Y : 0);

                         valuesList.Add(face324 != null ? face324.X : 0);
                         valuesList.Add(face324 != null ? face324.Y : 0);

                         valuesList.Add(face318 != null ? face318.X : 0);
                         valuesList.Add(face318 != null ? face318.Y : 0);

                         valuesList.Add(face402 != null ? face402.X : 0);
                         valuesList.Add(face402 != null ? face402.Y : 0);
                         
                         valuesList.Add(face317 != null ? face317.X : 0);
                         valuesList.Add(face317 != null ? face317.Y : 0);

                         valuesList.Add(face14 != null ? face14.X : 0);
                         valuesList.Add(face14 != null ? face14.Y : 0);

                         valuesList.Add(face87 != null ? face87.X : 0);
                         valuesList.Add(face87 != null ? face87.Y : 0);

                         valuesList.Add(face178 != null ? face178.X : 0);
                         valuesList.Add(face178 != null ? face178.Y : 0);

                         valuesList.Add(face95 != null ? face95.X : 0);
                         valuesList.Add(face95 != null ? face95.Y : 0);

                         valuesList.Add(face185 != null ? face185.X : 0);
                         valuesList.Add(face185 != null ? face185.Y : 0);

                         valuesList.Add(face40 != null ? face40.X : 0);
                         valuesList.Add(face40 != null ? face40.Y : 0);

                         valuesList.Add(face39 != null ? face39.X : 0);
                         valuesList.Add(face39 != null ? face39.Y : 0);

                         valuesList.Add(face37 != null ? face37.X : 0);
                         valuesList.Add(face37 != null ? face37.Y : 0);

                         valuesList.Add(face0 != null ? face0.X : 0);
                         valuesList.Add(face0 != null ? face0.Y : 0);

                         valuesList.Add(face267 != null ? face267.X : 0);
                         valuesList.Add(face267 != null ? face267.Y : 0);

                         valuesList.Add(face269 != null ? face269.X : 0);
                         valuesList.Add(face269 != null ? face269.Y : 0);
                         
                         valuesList.Add(face270 != null ? face270.X : 0);
                         valuesList.Add(face270 != null ? face270.Y : 0);

                         valuesList.Add(face409 != null ? face409.X : 0);
                         valuesList.Add(face409 != null ? face409.Y : 0);

                         valuesList.Add(face415 != null ? face415.X : 0);
                         valuesList.Add(face415 != null ? face415.Y : 0);

                         valuesList.Add(face310 != null ? face310.X : 0);
                         valuesList.Add(face310 != null ? face310.Y : 0);

                         valuesList.Add(face311 != null ? face311.X : 0);
                         valuesList.Add(face311 != null ? face311.Y : 0);

                         valuesList.Add(face312 != null ? face312.X : 0);
                         valuesList.Add(face312 != null ? face312.Y : 0);

                         valuesList.Add(face13 != null ? face13.X : 0);
                         valuesList.Add(face13 != null ? face13.Y : 0);

                         valuesList.Add(face82 != null ? face82.X : 0);
                         valuesList.Add(face82 != null ? face82.Y : 0);

                         valuesList.Add(face81 != null ? face81.X : 0);
                         valuesList.Add(face81 != null ? face81.Y : 0);

                         valuesList.Add(face42 != null ? face42.X : 0);
                         valuesList.Add(face42 != null ? face42.Y : 0);

                         valuesList.Add(face183 != null ? face183.X : 0);
                         valuesList.Add(face183 != null ? face183.Y : 0);

                        //

                        valuesTest.Add(face61 != null ? face61.X : 0);
                        valuesTest.Add(face61 != null ? face61.Y : 0);

                        valuesTest.Add(face146 != null ? face146.X : 0);
                        valuesTest.Add(face146 != null ? face146.Y : 0);

                        valuesTest.Add(face91 != null ? face91.X : 0);
                        valuesTest.Add(face91 != null ? face91.Y : 0);

                        valuesTest.Add(face181 != null ? face181.X : 0);
                        valuesTest.Add(face181 != null ? face181.Y : 0);

                        valuesTest.Add(face84 != null ? face84.X : 0);
                        valuesTest.Add(face84 != null ? face84.Y : 0);

                        valuesTest.Add(face17 != null ? face17.X : 0);
                        valuesTest.Add(face17 != null ? face17.Y : 0);

                        valuesTest.Add(face314 != null ? face314.X : 0);
                        valuesTest.Add(face314 != null ? face314.Y : 0);

                        valuesTest.Add(face405 != null ? face405.X : 0);
                        valuesTest.Add(face405 != null ? face405.Y : 0);

                        valuesTest.Add(face321 != null ? face321.X : 0);
                        valuesTest.Add(face321 != null ? face321.Y : 0);

                        valuesTest.Add(face375 != null ? face375.X : 0);
                        valuesTest.Add(face375 != null ? face375.Y : 0);

                        valuesTest.Add(face291 != null ? face291.X : 0);
                        valuesTest.Add(face291 != null ? face291.Y : 0);

                        valuesTest.Add(face308 != null ? face308.X : 0);
                        valuesTest.Add(face308 != null ? face308.Y : 0);

                        valuesTest.Add(face324 != null ? face324.X : 0);
                        valuesTest.Add(face324 != null ? face324.Y : 0);

                        valuesTest.Add(face318 != null ? face318.X : 0);
                        valuesTest.Add(face318 != null ? face318.Y : 0);

                        valuesTest.Add(face402 != null ? face402.X : 0);
                        valuesTest.Add(face402 != null ? face402.Y : 0);

                        valuesTest.Add(face317 != null ? face317.X : 0);
                        valuesTest.Add(face317 != null ? face317.Y : 0);

                        valuesTest.Add(face14 != null ? face14.X : 0);
                        valuesTest.Add(face14 != null ? face14.Y : 0);

                        valuesTest.Add(face87 != null ? face87.X : 0);
                        valuesTest.Add(face87 != null ? face87.Y : 0);

                        valuesTest.Add(face178 != null ? face178.X : 0);
                        valuesTest.Add(face178 != null ? face178.Y : 0);

                        valuesTest.Add(face88 != null ? face88.X : 0);
                        valuesTest.Add(face88 != null ? face88.Y : 0);

                        valuesTest.Add(face95 != null ? face95.X : 0);
                        valuesTest.Add(face95 != null ? face95.Y : 0);

                        valuesTest.Add(face185 != null ? face185.X : 0);
                        valuesTest.Add(face185 != null ? face185.Y : 0);

                        valuesTest.Add(face40 != null ? face40.X : 0);
                        valuesTest.Add(face40 != null ? face40.Y : 0);

                        valuesTest.Add(face39 != null ? face39.X : 0);
                        valuesTest.Add(face39 != null ? face39.Y : 0);

                        valuesTest.Add(face37 != null ? face37.X : 0);
                        valuesTest.Add(face37 != null ? face37.Y : 0);

                        valuesTest.Add(face0 != null ? face0.X : 0);
                        valuesTest.Add(face0 != null ? face0.Y : 0);

                        valuesTest.Add(face267 != null ? face267.X : 0);
                        valuesTest.Add(face267 != null ? face267.Y : 0);

                        valuesTest.Add(face269 != null ? face269.X : 0);
                        valuesTest.Add(face269 != null ? face269.Y : 0);

                        valuesTest.Add(face270 != null ? face270.X : 0);
                        valuesTest.Add(face270 != null ? face270.Y : 0);

                        valuesTest.Add(face409 != null ? face409.X : 0);
                        valuesTest.Add(face409 != null ? face409.Y : 0);

                        valuesTest.Add(face415 != null ? face415.X : 0);
                        valuesTest.Add(face415 != null ? face415.Y : 0);

                        valuesTest.Add(face310 != null ? face310.X : 0);
                        valuesTest.Add(face310 != null ? face310.Y : 0);

                        valuesTest.Add(face311 != null ? face311.X : 0);
                        valuesTest.Add(face311 != null ? face311.Y : 0);

                        valuesTest.Add(face312 != null ? face312.X : 0);
                        valuesTest.Add(face312 != null ? face312.Y : 0);

                        valuesTest.Add(face13 != null ? face13.X : 0);
                        valuesTest.Add(face13 != null ? face13.Y : 0);

                        valuesTest.Add(face82 != null ? face82.X : 0);
                        valuesTest.Add(face82 != null ? face82.Y : 0);

                        valuesTest.Add(face81 != null ? face81.X : 0);
                        valuesTest.Add(face81 != null ? face81.Y : 0);

                        valuesTest.Add(face42 != null ? face42.X : 0);
                        valuesTest.Add(face42 != null ? face42.Y : 0);

                        valuesTest.Add(face183 != null ? face183.X : 0);
                        valuesTest.Add(face183 != null ? face183.Y : 0);

                        valuesTest.Add(face78 != null ? face78.X : 0);
                        valuesTest.Add(face78 != null ? face78.Y : 0);

                        //Vector2 poseLandmarkValues = new Vector2(Dot10.x, Dot10.y);
                        //poseLandmarkValuesList.Add(poseLandmarkValues); 
                    }
                }
            }


           // Debug.Log("All Values: " + string.Join(", ", valuesList));



            SpeedConfig speedConfig = new SpeedConfig
            {
                lento = 15,
                medio = 12,
                rapido = 8
            };

            

            LandmarkItem landmarkItem = new LandmarkItem ();
            if (valuesList.Count == 136) { 
            for (int i = 0; i < valuesList.Count; i++)
            {
            if (i < 12)
            {
        
             if (landmarkItem.pose_landmarks == null)
            landmarkItem.pose_landmarks = new List<float>();
            landmarkItem.pose_landmarks.Add(valuesList[i]);
            }
            else if (i < 36)
            {
        
             if (landmarkItem.left_hand_landmarks == null)
             landmarkItem.left_hand_landmarks = new List<float>();
             landmarkItem.left_hand_landmarks.Add(valuesList[i]);
            }
             else if (i < 60)
            {
            
            if (landmarkItem.right_hand_landmarks == null)
            landmarkItem.right_hand_landmarks = new List<float>();
            landmarkItem.right_hand_landmarks.Add(valuesList[i]);
            }
            else if (i < 137)
            {
        
            if (landmarkItem.face_landmarks == null)
            landmarkItem.face_landmarks = new List<float>();
            landmarkItem.face_landmarks.Add(valuesList[i]);
            }
            }
            }
            Final FinalObject = new Final
            {
                speed = speedConfig,
                mp = landmarkItem,
                mp_detection_mandatory = new List<string>
                {
                    "left_hand_landmarks",
                    "right_hand_landmarks"
                }
            };
            
            Debug.Log("COUNT:" + valuesTest.Count);

            TextResult.text = "COUNT: " + valuesTest.Count;


            Landmarks landmarks = new Landmarks();
            if (valuesTest.Count == 176)
            {
                for (int i = 0; i < valuesTest.Count; i++)
                {
                    if (i < 12)
                    {
                        if (landmarks.pose_landmarks == null)
                            landmarks.pose_landmarks = new List<float>();
                        landmarks.pose_landmarks.Add(valuesTest[i]);
                        
                    }
                    else if (i < 54)
                    {
                        if (landmarks.left_hand_landmarks == null)
                            landmarks.left_hand_landmarks = new List<float>();
                        landmarks.left_hand_landmarks.Add(valuesTest[i]);
                        
                    }
                    else if (i < 96)
                    {
                        if (landmarks.right_hand_landmarks == null)
                            landmarks.right_hand_landmarks = new List<float>();
                        landmarks.right_hand_landmarks.Add(valuesTest[i]);
                        
                    }
                    else if (i < 176)
                    {
                        if (landmarks.face_landmarks == null)
                            landmarks.face_landmarks = new List<float>();

                        landmarks.face_landmarks.Add(valuesTest[i]);
                        
                    }
                }
            }


            var requestData = new PostRequestData
            {
                vocab_model_opts = new VocabModelOpts
                {
                    Final = new string[] { "model2-OLD_ConvLSTM1D", "Final", "model2_ConvLSTM1D" },
                    Compras = new string[] { "Dataset_2_Compras_model2_ConvLSTM1D", "Dataset_2_Compras_model1_SimpleLSTM" }
                },
                vocab_selected = "Final",
                model_selected = "model2_ConvLSTM1D",
                landmarks = landmarks,
                collectnumber = new CollectNumber
                {
                    lento = 15,
                    medio = 12,
                    rapido = 8
                }
            };
            
            
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 1)
            {
                valuesTest1.AddRange(valuesTest);
                Debug.Log("1 " + string.Join(", ", valuesTest1));
                Debug.Log("1 count " + valuesTest1.Count);
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 2)
            {
                valuesTest2.AddRange(valuesTest);
                Debug.Log("2 " + string.Join(", ", valuesTest2));
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 3)
            {
                valuesTest3.AddRange(valuesTest);
                Debug.Log("3 " + string.Join(", ", valuesTest3));
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 4)
            {
                valuesTest4.AddRange(valuesTest);
                Debug.Log("4 " + string.Join(", ", valuesTest4));
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 5)
            {
                valuesTest5.AddRange(valuesTest);
                Debug.Log("5 " + string.Join(", ", valuesTest5));
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 6)
            {
                valuesTest6.AddRange(valuesTest);
                Debug.Log("6 " + string.Join(", ", valuesTest6));
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 7)
            {
                valuesTest7.AddRange(valuesTest);
                Debug.Log("7 " + string.Join(", ", valuesTest7));
            }
            if (valuesTest.Count == 176 && (valuesTestCount % 8) == 0)
            {
                valuesTest8.AddRange(valuesTest);
                Debug.Log("8 " + string.Join(", ", valuesTest8));
            }

            
            if (valuesTest.Count == 176 && valuesTestCount % 8 == 0)
            {
                List<List<float>> listOfLists = new List<List<float>>();

                // Add the individual lists to the list of lists
                listOfLists.Add(valuesTest1);
                listOfLists.Add(valuesTest2);
                listOfLists.Add(valuesTest3);
                listOfLists.Add(valuesTest4);
                listOfLists.Add(valuesTest5);
                listOfLists.Add(valuesTest6);
                listOfLists.Add(valuesTest7);
                listOfLists.Add(valuesTest8);

                json = "[";
                
                foreach (var list in listOfLists)
                {
                    json += "[";
                    for (int i = 0; i < list.Count; i++)
                    {
                        json += list[i].ToString();
                        if (i < list.Count - 1)
                        {
                            json += ",";
                        }
                    }
                    json += "]";

                    // Add a comma if it's not the last element and the counter is less than 7
                    if (counter < 7 && list != listOfLists.Last())
                    {
                        json += ",";
                    }

                    counter++;
                }

                json += "]";

                json = "[" + json + "]";

                Debug.Log("LISTAAAAAAAAA" + json);
                //string jsondatalist = JsonUtility.ToJson(finalList);
                //Debug.Log("jsondata:" + jsondatalist);
                //TextResult.text = "jsondata:" + jsondatalist;
                counter = 0;
            }


            if (valuesTest.Count == 176 && valuesTestCount % 8 == 0)
            {
                string jsondata = JsonUtility.ToJson(requestData);
                Debug.Log("jsondata:" + jsondata);
                //TextResult.text = "jsondata:" + jsondata;
            }

            //string jsondata = JsonUtility.ToJson(requestData);
            //Debug.Log("jsondata:" + jsondata);

            //TextResult.text = "jsondata:" + jsondata;



            /*if (valuesTestCount > 1)
            {
                if (networkManagerObject != null)
                {
                    networkManager = networkManagerObject.GetComponent<NetworkManager>();
                }
                else
                {
                    Debug.LogError("NetworkManagerObject not found in the scene!");
                }

                if (networkManager != null)
                {
                    StartCoroutine(networkManager.SendPostRequest(jsondata));
                }
            }*/




            //
            /*
            Debug.Log("a" + valuesList.Count);

            string jsonteste = JsonUtility.ToJson(landmarkItem);
          //  Debug.Log("Teste: " + jsonteste);

            string jsonfinal = JsonUtility.ToJson(FinalObject);
            Debug.Log("!!!!!!!!!!!!!!!!!!: " + jsonfinal);
            

            FloatListWrapper wrapper = new FloatListWrapper(valuesList);
            string json = JsonUtility.ToJson(wrapper);
            //Debug.Log("JSON: " + json);


            //NetworkingManager networkingManager = GetComponent<NetworkingManager>();
            // yield return networkingManager.SendPostRequest(json);


            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //networkManager = new NetworkManager();
            //StartCoroutine(networkManager.SendPostRequest(json));

            */


            if (Time.time - lastRequestTime > 5.0f) 
            {
                lastRequestTime = Time.time; 

                if (canSendRequest && valuesTest.Count == 176 && json != null && valuesTest1 != null && valuesTest2 != null &&
                    valuesTest3 != null && valuesTest4 != null && valuesTest5 != null && valuesTest6 != null &&
                    valuesTest7 != null && valuesTest8 != null)
                {
                    NetworkManager networkManager = networkManagerObject.GetComponent<NetworkManager>();

                    if (networkManager != null)
                    {
                        StartCoroutine(networkManager.SendPostRequest(json, OnResponseReceived));
                        
                    }
                    else
                    {
                        Debug.LogError("NetworkManager component not found on GameObject.");
                    }
                }
            }


            ///////////////////////////////////////////////////////////////////////
            /*if (valuesTest.Count == 176 && json != null && valuesTest1 != null && valuesTest2 != null && valuesTest3 != null && valuesTest4 != null && valuesTest5 != null && valuesTest6 != null && valuesTest7 != null && valuesTest8 != null)
            {
                NetworkManager networkManager = networkManagerObject.GetComponent<NetworkManager>();

                if (networkManager != null)
                {
                    StartCoroutine(networkManager.SendPostRequest(json, OnResponseReceived));
                }
                else
                {
                    Debug.LogError("NetworkManager component not found on GameObject.");
                }

                //proxySocket = new ProxySocket();
               // StartCoroutine(proxySocket.SendHttpRequest(json));
;
            }*/
            //////////////////////////////////////////////////////////////////////


            //Debug.Log("valuesTestCount " + valuesTestCount);


            if (valuesTest.Count == 176 && valuesTestCount % 8 == 0)
            {
                valuesTest1.Clear();
                valuesTest2.Clear();
                valuesTest3.Clear();
                valuesTest4.Clear();
                valuesTest5.Clear();
                valuesTest6.Clear();
                valuesTest7.Clear();
                valuesTest8.Clear();
            }
            else
            {
                Console.WriteLine("The count is not a multiple of 8 or valuesTest != 176.");
            }

            TextResult.text = valuesTest[0].ToString();

            

            if (valuesTest.Count == 176)
            {
                valuesTestCount++;
            }

            Debug.Log("count" + valuesTestCount);

            valuesList.Clear();

            valuesTest.Clear();

            /*
            if (faceLandmarksStream.TryGetNext(out var multiFaceLandmarks))
            {
                if (multiFaceLandmarks != null && multiFaceLandmarks.Count > 0)
                {
                    foreach (var landmarks in multiFaceLandmarks)
                    {
                       // _multiFaceLandmarksAnnotationController.DrawNow(multiFaceLandmarks);
                        // top of the head
                        var topOfHead = landmarks.Landmark[10];
                        var Dot1 = landmarks.Landmark[61];
                        var Dot2 = landmarks.Landmark[146];
                        Debug.Log($"Face: {Dot1} ; {topOfHead}");
                    }
                }
            }/*
            else
            {
               // _multiFaceLandmarksAnnotationController.DrawNow(null);
            }*/

            /*

            if (lefthandLandmarksStream.TryGetNext(out var lefthandLandmarks))
            {
                if (lefthandLandmarks != null && lefthandLandmarks.Count > 0)
                {
                    foreach (var landmarks in lefthandLandmarks)
                    {
                      //  _multiHandLandmarksAnnotationController.DrawNow(lefthandLandmarks);
                        var Dot3 = landmarks.Landmark[5];
                        Debug.Log($"Lefthand: {Dot3}");
                    }
                }
            }
            else
            {
               // _multiHandLandmarksAnnotationController.DrawNow(null);
            }

            if (righthandLandmarksStream.TryGetNext(out var righthandLandmarks))
            {
                if (righthandLandmarks != null && righthandLandmarks.Count > 0)
                {
                    foreach (var landmarks in righthandLandmarks)
                    {
                     //   _multiHandLandmarksAnnotationController.DrawNow(righthandLandmarks);
                        var Dot3 = landmarks.Landmark[6];
                        Debug.Log($"Righthand: {Dot3}");
                    }
                }
            }
            else
            {
               // _multiHandLandmarksAnnotationController.DrawNow(null);
            }

            */
        }
        



    }

    private void OnResponseReceived(string response)
    {

        //Debug.Log("Response from server: " + response);

        int startIndex = response.IndexOf(":\"");

        if (startIndex != -1)
        {
            int endIndex = response.IndexOf("\"", startIndex + 2);

            if (endIndex != -1)
            {
                string value = response.Substring(startIndex + 2, endIndex - startIndex - 2);

                Debug.Log("Response from server: " + value);
                //Result.text = value;

                if (Time.time - lastClearTime > clearInterval)
                {
                    combinedValueBuilder.Clear();
                    lastClearTime = Time.time;
                }

                if (value != previousValue)
                {
                    combinedValueBuilder.Append(value).Append(" ");

                    previousValue = value;

                    Result.text = combinedValueBuilder.ToString();

                }
                else
                {
                    Result.text = combinedValueBuilder.ToString();
                }
            }
            else
            {
                Debug.LogError("Closing double quote not found.");
            }
        }
        else
        {
            Debug.LogError("Colon and starting double quote not found.");
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
       


