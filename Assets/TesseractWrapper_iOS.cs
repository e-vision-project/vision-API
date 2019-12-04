using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;  
using System.Runtime.InteropServices; 

using System;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;

public class TesseractWrapper_iOS: MonoBehaviour {
	[DllImport ("__Internal")]
	private static extern string TesseractOCRVersion_iOS();

	[DllImport ("__Internal")]
	private static extern string MakeTesseractPhoto_iOS (string lang, string charEncodeData); 

	[DllImport ("__Internal")]
	private static extern string MakeTesseractPhotoFromMemory_iOS (string lang, string dataBytes); 

	[DllImport ("__Internal")]
	private static extern void TessBaseAPICreate_iOS (string dataPath, string lang); 

	[DllImport ("__Internal")]
	private static extern void TessBaseAPIDelete_iOS (string lang); 

	private string language = "";

	public string Version()
	{
		string str_ptr = TesseractOCRVersion_iOS();
		return str_ptr;

	}

	public string RecognizeFromFile(string filename, int procCount) 
	{
		if (language.Length == 0) {
			Init("eng", "");
		}
		string tesseractRecognizeResult = MakeTesseractPhoto_iOS(language, filename);
		return tesseractRecognizeResult;

	}

	public string Recognize (Color32[] colors, int width, int height)
	{
		
		Texture2D workTex = new Texture2D(width, height,TextureFormat.ARGB32, false);
		workTex.SetPixels32(colors);
		workTex.Apply();

		string stringData = Convert.ToBase64String (workTex .EncodeToPNG ()); 

		return MakeTesseractPhotoFromMemory_iOS (language, stringData);

	}

	public string RecognizeFromTexture(Texture2D texture, bool doCopy)
	{
		Texture2D workTex = texture;
		if(doCopy)
		{
			workTex = new Texture2D(texture.width, texture.height,TextureFormat.ARGB32, false);
			workTex.SetPixels32(texture.GetPixels32());
			workTex.Apply();
		}
		return Recognize(workTex.GetPixels32(),workTex.width, workTex.height);
	}

	public bool Init(string lang, string dataPath)
	{
		
		if (language.Length != 0) {
			Close ();
		}

		TessBaseAPICreate_iOS (dataPath, lang);
		language = lang;

		if (language.Length == 0) {
			return false;
		}
		return true;
	}

	public void Close()
	{
		if (language.Length == 0)
			return;
		TessBaseAPIDelete_iOS(language);
		language = "";
	}
}

