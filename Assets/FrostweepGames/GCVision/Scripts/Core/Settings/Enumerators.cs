namespace FrostweepGames.Plugins.GoogleCloud.Vision
{
    public class Enumerators
    {
        public enum FeatureType
        {
            TYPE_UNSPECIFIED,
            FACE_DETECTION,
            LANDMARK_DETECTION,
            LOGO_DETECTION,
            LABEL_DETECTION,
            TEXT_DETECTION,
            DOCUMENT_TEXT_DETECTION,
            SAFE_SEARCH_DETECTION,
            IMAGE_PROPERTIES,
            CROP_HINTS,
            WEB_DETECTION,
            PRODUCT_SEARCH,
            OBJECT_LOCALIZATION
        }

        public enum LandmarkType
        {
            UNKNOWN_LANDMARK,
            LEFT_EYE,
            RIGHT_EYE,
            LEFT_OF_LEFT_EYEBROW,
            RIGHT_OF_LEFT_EYEBROW,
            LEFT_OF_RIGHT_EYEBROW,
            RIGHT_OF_RIGHT_EYEBROW,
            MIDPOINT_BETWEEN_EYES,
            NOSE_TIP,
            UPPER_LIP,
            LOWER_LIP,
            MOUTH_LEFT,
            MOUTH_RIGHT,
            MOUTH_CENTER,
            NOSE_BOTTOM_RIGHT,
            NOSE_BOTTOM_LEFT,
            NOSE_BOTTOM_CENTER,
            LEFT_EYE_TOP_BOUNDARY,
            LEFT_EYE_RIGHT_CORNER,
            LEFT_EYE_BOTTOM_BOUNDARY,
            LEFT_EYE_LEFT_CORNER,
            RIGHT_EYE_TOP_BOUNDARY,
            RIGHT_EYE_RIGHT_CORNER,
            RIGHT_EYE_BOTTOM_BOUNDARY,
            RIGHT_EYE_LEFT_CORNER,
            LEFT_EYEBROW_UPPER_MIDPOINT,
            RIGHT_EYEBROW_UPPER_MIDPOINT,
            LEFT_EAR_TRAGION,
            RIGHT_EAR_TRAGION,
            LEFT_EYE_PUPIL,
            RIGHT_EYE_PUPIL,
            FOREHEAD_GLABELLA,
            CHIN_GNATHION,
            CHIN_LEFT_GONION,
            CHIN_RIGHT_GONION
        }

        public enum Likelihood
        {
            UNKNOWN,
            VERY_UNLIKELY,
            UNLIKELY,
            POSSIBLE,
            LIKELY,
            VERY_LIKELY
        }

        public enum BreakType
        {
            UNKNOWN,
            SPACE,
            SURE_SPACE,
            EOL_SURE_SPACE,
            HYPHEN,
            LINE_BREAK
        }

        public enum BlockType
        {
            UNKNOWN,
            TEXT,
            TABLE,
            PICTURE,
            RULER,
            BARCODE
        }


        public enum ResponseDataType
        {
            LABELS,
            LOGOS,
            WEB,
            TEXT,
            DOCUMENT,
            PROPERTIES,
            SAFE_SEARCH
        }

        public enum DataType
        {
            WEB_ENTITIES,
            PAGES_WITH_MATHCHED_IMAGES,
            FULLY_MATCHED_IMAGES,
            PARTIALLY_MATCHED_IMAGES,
            VISUAL_SIMILAR_IMAGES,
            TEXT,
            LABELS,
            LOGOS,
            DOMINANT_COLORS,
            CROP_HINTS,
            SAFE_SEARCHS,
            PAGES,
            BLOCKS,
            PARAGRAPH
        }
        
        public enum SystemItemType
        {
            UNKNOWN,
            FILE,
            FOLDER,
            CD,
            FLASH
        }
    }
}