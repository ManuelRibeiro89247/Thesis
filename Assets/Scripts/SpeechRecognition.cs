using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using System.Threading.Tasks;
using System.Globalization;
using System;
using System.Diagnostics;
using TMPro;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using Debug = UnityEngine.Debug;

public class SpeechRecognition : MonoBehaviour
{
    public Text RecognizedText;
    public Text ErrorText;

    public Toggle TranslationEnabled;
    public Dropdown Languages1;
    public Dropdown Languages2;
    public Dropdown Languages3;


    private string recognizedString = "Começar tradução";
    private string errorString = "";
    private System.Object threadLocker = new System.Object();

    public string SpeechServiceAPIKey = string.Empty;
    public string SpeechServiceRegion = "westeurope";

    private SpeechRecognizer recognizer;
    private TranslationRecognizer translator;

    string fromLanguage = "pt-pt";

    private bool micPermissionGranted = false;

    private void Start()
    {
        fromLanguageDropdown.onValueChanged.AddListener(OnFromLanguageDropdownValueChanged);
        micPermissionGranted = true;
    }

    public void StartContinuous()
    {
        errorString = "";
        if (micPermissionGranted)
        {
            if (TranslationEnabled.isOn)
            {
                StartContinuousTranslation();
            }
            else
            {
                StartContinuousRecognition();
            }
        }
        else
        {
            recognizedString = "Acesso ao microfone é obrigatório.";
            errorString = "ERROR: Acesso ao microfone rejeitado.";
            Debug.LogError(errorString);
        }
    }


    void CreateSpeechRecognizer()
    {
        if (SpeechServiceAPIKey.Length == 0 || SpeechServiceAPIKey == "YourSubscriptionKey")
        {
            recognizedString = "You forgot to obtain Cognitive Services Speech credentials and inserting them in this app." + Environment.NewLine +
                               "See the README file and/or the instructions in the Awake() function for more info before proceeding.";
            errorString = "ERROR: Missing service credentials";
            Debug.LogError(errorString);
            return;
        }
        Debug.Log("Creating Speech Recognizer.");
        recognizedString = "Initializing speech recognition, please wait...";

        if (recognizer == null)
        {
            SpeechConfig config = SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
            config.SpeechRecognitionLanguage = ExtractLanguageCode(fromLanguageDropdown.captionText.text);
            recognizer = new SpeechRecognizer(config);

            if (recognizer != null)
            {
                recognizer.Recognizing += RecognizingHandler;
                recognizer.Recognized += RecognizedHandler;
                recognizer.SpeechStartDetected += SpeechStartDetectedHandler;
                recognizer.SpeechEndDetected += SpeechEndDetectedHandler;
                recognizer.Canceled += CanceledHandler;
                recognizer.SessionStarted += SessionStartedHandler;
                recognizer.SessionStopped += SessionStoppedHandler;
            }
        }
        Debug.Log("CreateSpeechRecognizer exit");
    }

    private async void StartContinuousRecognition()
    {
        Debug.Log("Starting Continuous Speech Recognition.");
        CreateSpeechRecognizer();

        if (recognizer != null)
        {
            Debug.Log("Starting Speech Recognizer.");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            recognizedString = "Speech Recognizer está a correr.";
            Debug.Log("Speech Recognizer is now running.");
        }
        Debug.Log("Start Continuous Speech Recognition exit");
    }

    #region Speech Recognition event handlers
    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session started event. Event: {e.ToString()}.");
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session event. Event: {e.ToString()}.");
        Debug.Log($"Session Stop detected. Stop the recognition.");
    }

    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechStartDetected received: offset: {e.Offset}.");
    }

    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechEndDetected received: offset: {e.Offset}.");
        Debug.Log($"Speech end detected.");
    }

    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        {
            Debug.Log($"HYPOTHESIS: Text={e.Result.Text}");
            lock (threadLocker)
            {
                recognizedString = $"HYPOTHESIS: {Environment.NewLine}{e.Result.Text}";
            }
        }
    }

    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
            lock (threadLocker)
            {
                recognizedString = $"RESULT: {Environment.NewLine}{e.Result.Text}";
            }
        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            Debug.Log($"NOMATCH: Speech could not be recognized.");
        }
    }

    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        Debug.Log($"CANCELED: Reason={e.Reason}");

        errorString = e.ToString();
        if (e.Reason == CancellationReason.Error)
        {
            Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
            Debug.LogError("CANCELED: Did you update the subscription info?");
        }
    }
    #endregion


    void CreateTranslationRecognizer()
    {
        Debug.Log("Creating Translation Recognizer.");
        recognizedString = "Speech recognition a começar com tradução...";

        if (translator == null)
        {
            SpeechTranslationConfig config = SpeechTranslationConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
            config.SpeechRecognitionLanguage = fromLanguage;
            if (Languages1.captionText.text.Length > 0)
                config.AddTargetLanguage(ExtractLanguageCode(Languages1.captionText.text));
            if (Languages2.captionText.text.Length > 0)
                config.AddTargetLanguage(ExtractLanguageCode(Languages2.captionText.text));
            if (Languages3.captionText.text.Length > 0)
                config.AddTargetLanguage(ExtractLanguageCode(Languages3.captionText.text));
            translator = new TranslationRecognizer(config);

            if (translator != null)
            {
                translator.Recognizing += RecognizingTranslationHandler;
                translator.Recognized += RecognizedTranslationHandler;
                translator.SpeechStartDetected += SpeechStartDetectedHandler;
                translator.SpeechEndDetected += SpeechEndDetectedHandler;
                translator.Canceled += CanceledTranslationHandler;
                translator.SessionStarted += SessionStartedHandler;
                translator.SessionStopped += SessionStoppedHandler;
            }
        }
        Debug.Log("CreateTranslationRecognizer exit");
    }

    string ExtractLanguageCode(string languageListLabel)
    {
        return languageListLabel.Substring(0, languageListLabel.IndexOf("_"));
    }

    private async void StartContinuousTranslation()
    {
        Debug.Log("Starting Continuous Translation Recognition.");
        CreateTranslationRecognizer();

        if (translator != null)
        {
            Debug.Log("Starting Speech Translator.");
            await translator.StartContinuousRecognitionAsync().ConfigureAwait(false);

            recognizedString = "Speech Translator está a correr.";
            Debug.Log("Speech Translator is now running.");
        }
        Debug.Log("Start Continuous Speech Translation exit");
    }

    #region Speech Translation event handlers

    private void RecognizingTranslationHandler(object sender, TranslationRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.TranslatingSpeech)
        {
            Debug.Log($"RECOGNIZED HYPOTHESIS: Text={e.Result.Text}");
            lock (threadLocker)
            {
                //recognizedString = $"RECOGNIZED HYPOTHESIS ({fromLanguage}): {Environment.NewLine}{e.Result.Text}";
                //recognizedString += $"{Environment.NewLine}TRANSLATED HYPOTHESESE:";
                foreach (var element in e.Result.Translations)
                {
                    //recognizedString += $"{Environment.NewLine}[{element.Key}]: {element.Value}";
                }
            }
        }
    }

    private void RecognizedTranslationHandler(object sender, TranslationRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.TranslatedSpeech)
        {
            Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
            lock (threadLocker)
            {
                if (fromLanguageDropdown.captionText.text == "Português")
                {
                    recognizedString = $"Texto reconhecido ({fromLanguage}): {Environment.NewLine}{e.Result.Text}";
                }
                else
                {
                    recognizedString = $"Texto reconhecido ({fromLanguage}): {Environment.NewLine}{e.Result.Text}";
                    recognizedString += $"{Environment.NewLine}Resultado:";
                    foreach (var element in e.Result.Translations)
                    {
                        recognizedString += $"{Environment.NewLine}[{element.Key}]: {element.Value}";
                    }
                }

            }
        }
        else if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
            lock (threadLocker)
            {
                recognizedString = $"RESULTADO NÃO TRADUZIDO: {Environment.NewLine}{e.Result.Text}";
            }
        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            Debug.Log($"NOMATCH: Speech could not be recognized or translated.");
        }
    }


    private void CanceledTranslationHandler(object sender, TranslationRecognitionCanceledEventArgs e)
    {
        Debug.Log($"CANCELED: Reason={e.Reason}");

        errorString = e.ToString();
        if (e.Reason == CancellationReason.Error)
        {
            Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
            Debug.LogError($"CANCELED: Did you update the subscription info?");
        }
    }
    #endregion

    void Update()
    {
        lock (threadLocker)
        {
            RecognizedText.text = recognizedString;
            RecognizedText2.text = recognizedString;
            ErrorText.text = errorString;
        }
    }

    void OnDisable()
    {
        StopRecognition();
    }

    public async void StopRecognition()
    {
        if (recognizer != null)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Recognizing -= RecognizingHandler;
            recognizer.Recognized -= RecognizedHandler;
            recognizer.SpeechStartDetected -= SpeechStartDetectedHandler;
            recognizer.SpeechEndDetected -= SpeechEndDetectedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.SessionStarted -= SessionStartedHandler;
            recognizer.SessionStopped -= SessionStoppedHandler;
            recognizer.Dispose();
            recognizer = null;
            recognizedString = "Speech Recognizer is now stopped.";
            Debug.Log("Speech Recognizer is now stopped.");
        }
        if (translator != null)
        {
            await translator.StopContinuousRecognitionAsync().ConfigureAwait(false);
            translator.Recognizing -= RecognizingTranslationHandler;
            translator.Recognized -= RecognizedTranslationHandler;
            translator.SpeechStartDetected -= SpeechStartDetectedHandler;
            translator.SpeechEndDetected -= SpeechEndDetectedHandler;
            translator.Canceled -= CanceledTranslationHandler;
            translator.SessionStarted -= SessionStartedHandler;
            translator.SessionStopped -= SessionStoppedHandler;
            translator.Dispose();
            translator = null;
            recognizedString = "Começar tradução";
            Debug.Log("Speech Translator is now stopped.");
        }
    }
    //extra
    public Dropdown fromLanguageDropdown;
    string fromLanguageDropdownText;
    public TextMeshProUGUI RecognizedText2;

    void OnFromLanguageDropdownValueChanged(int index)
    {

        fromLanguageDropdownText = fromLanguageDropdown.options[index].text;
        if (fromLanguageDropdownText == "Português")
        {
            fromLanguage = "pt-pt";
        }
        if (fromLanguageDropdownText == "Espanhol")
        {
            fromLanguage = "es-es";
        }
        if (fromLanguageDropdownText == "Francês")
        {
            fromLanguage = "fr-fr";
        }
    }
}