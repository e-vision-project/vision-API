using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;

namespace EVISION.Camera.plugin
{
    public class CloudTextToSpeechSynthesis : MonoBehaviour, ITextToVoice
    {
        [SerializeField] private AudioSource audioSource;
        private GCTextToSpeech _gcTextToSpeech;

        private Voice[] _voices;
        private Voice _currentVoice;

        public string text;

        public void PerformSpeechFromText()
        {
            GetVoicesHandler(Enumerators.LanguageCode.el_GR);
            SynthesizeHandler(text);
        }

        #region MonoBehaviour callbacks
        // Start is called before the first frame update
        void Start()
        {
            _gcTextToSpeech = GCTextToSpeech.Instance;

            _gcTextToSpeech.GetVoicesSuccessEvent += _gcTextToSpeech_GetVoicesSuccessEvent;
            _gcTextToSpeech.SynthesizeSuccessEvent += _gcTextToSpeech_SynthesizeSuccessEvent;

            _gcTextToSpeech.GetVoicesFailedEvent += _gcTextToSpeech_GetVoicesFailedEvent;
            _gcTextToSpeech.SynthesizeFailedEvent += _gcTextToSpeech_SynthesizeFailedEvent;

        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion

        private void GetVoicesHandler(Enumerators.LanguageCode lang)
        {
            Debug.Log("asdasd");
            _gcTextToSpeech.GetVoices(new GetVoicesRequest()
            {
                languageCode = _gcTextToSpeech.PrepareLanguage(Enumerators.LanguageCode.el_GR)
                Debug.Log(languageCode);
            });
        }

        private void SynthesizeHandler(string textContent)
        {
            Debug.Log("1");
            string content = textContent;
            if (string.IsNullOrEmpty(content))
                return;
            Debug.Log("2");
            _gcTextToSpeech.Synthesize(content, new VoiceConfig()
            {
                gender = Enumerators.SsmlVoiceGender.MALE,
                languageCode = _currentVoice.languageCodes[0],
                name = _currentVoice.name
            },
            false,
            1,
            1,
            16000);
        }

        private void FillVoicesList()
        {
            if (_voices == null)
                return;

            List<string> elements = new List<string>();

            for (int i = 0; i < _voices.Length; i++)
            {
                if (_voices[i].name.ToLower().Contains(((Enumerators.VoiceType.WAVENET)).ToString().ToLower()))
                    elements.Add(_voices[i].name);
            }
        }

        #region failed handlers

        private void _gcTextToSpeech_SynthesizeFailedEvent(string error)
        {
            Debug.Log(error);
        }

        private void _gcTextToSpeech_GetVoicesFailedEvent(string error)
        {
            Debug.Log(error);
        }

        #endregion failed handlers

        #region success handlers

        private void _gcTextToSpeech_SynthesizeSuccessEvent(PostSynthesizeResponse response)
        {
            Debug.Log("success 2");
            audioSource.clip = _gcTextToSpeech.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
            audioSource.Play();
        }

        private void _gcTextToSpeech_GetVoicesSuccessEvent(GetVoicesResponse response)
        {
            _voices = response.voices;
            FillVoicesList();
        }


        #endregion sucess handlers
    }
}
