namespace FrostweepGames.Plugins.GoogleCloud.Translation
{
    public class Enumerators
    {
        public enum TextFormatType
        {
            html,
            text
        }

        public enum GoogleCloudRequestType
        {
            TRANSLATE,
            DETECT_LANGUAGE,
            GET_LANGUAGES
        }

        public enum ModelType
        {
            @base, // Phrase-Based Machine Translation (PBMT)
            nmt // Neural Machine Translation (NMT)
        }

        public enum TextLanguage
        {
            RU,
            EN,
            DE,
            FR,
            UK,
            AF,
            AR,
            EU,
            BE,
            BG,
            HR,
            ZH,
            CS,
            DA,
            HL,
            ET,
            FO,
            FI,
            EL,
            HE,
            HU,
            IS,
            ID,
            IT,
            JA,
            KO,
            LV,
            LT,
            NO,
            PL,
            PT,
            RO,
            SK,
            SL,
            ES,
            SV,
            TH,
            TR,
            VI
        }

    }
}