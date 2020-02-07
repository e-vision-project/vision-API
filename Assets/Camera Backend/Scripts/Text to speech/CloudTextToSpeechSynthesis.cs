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
        private List<string> voices = new List<string>();
        private Voice _currentVoice;
        [SerializeField] private Enumerators.LanguageCode currentLanguage;
        private bool speechSynthesisCompleted = false;

        public IEnumerator PerformSpeechFromText(string voiceOverText)
        {
            SynthesizeHandler(voiceOverText);
            while (speechSynthesisCompleted != true)
            {
                yield return null;
            }
        }

        public void StopSpeech()
        {
            audioSource.Stop();
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
            GetVoicesHandler();

        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion

        private void GetVoicesHandler()
        {
            Debug.Log("Get voices");
            _gcTextToSpeech.GetVoices(new GetVoicesRequest()
            {
                languageCode = _gcTextToSpeech.PrepareLanguage(currentLanguage)
            });
        }

        private void SynthesizeHandler(string contentText)
        {
            string content = contentText;
            var voice = _voices.ToList().Find(item => item.name.Contains(voices[0]));
            _currentVoice = voice;
            if (string.IsNullOrEmpty(content) || _currentVoice == null)
                return;

            _gcTextToSpeech.Synthesize(content, new VoiceConfig()
            {
                gender = _currentVoice.ssmlGender,
                languageCode = _currentVoice.languageCodes[0],
                name = _currentVoice.name
            }
            );
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
            voices.AddRange(elements);
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
            audioSource.clip = _gcTextToSpeech.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
            speechSynthesisCompleted = true;
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