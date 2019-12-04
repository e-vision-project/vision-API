using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesseractWrapper : MonoBehaviour {
	
	#if UNITY_IOS
	private TesseractWrapper_iOS tesseractClass = new TesseractWrapper_iOS ();
	#elif UNITY_ANDROID
	private TesseractWrapper_And tesseractClass = new TesseractWrapper_And ();
	#endif

	//Method returns a version of Tesseract. May be used for testing.
	public string Version()
	{
		return tesseractClass.Version ();

	}

	//Method returns text recognized from the image file in filename.
	public string RecognizeFromFile(string filename)
	{
	
		int procCount = 0; 
		return tesseractClass.RecognizeFromFile (filename, procCount);

	}
		

	//Method returns text recognized from an array of colors with width and height (in points).  
	public string Recognize (Color32[] colors, int width, int height)
	{

		return tesseractClass.Recognize (colors, width, height);

	}

	//Method returns text recognized text from a texture. doCopy allows source texture to be changed while recognizing performs
	public string RecognizeFromTexture(Texture2D texture, bool doCopy)
	{
		return tesseractClass.RecognizeFromTexture (texture, doCopy);

	}

	//Method inits a tesseractObject with language lang (in Tesseract language format, for example "en"  or "en+ru"). 
	//dataPath is a path to a custom tessData folder, if null, the path to default tessData folder is used (recommended)
	public bool Init(string lang, string dataPath)
	{
		return tesseractClass.Init (lang, dataPath);

	}

	//Method deinits tesseractObject
	public void Close()
	{
		tesseractClass.Close ();


	}
}
