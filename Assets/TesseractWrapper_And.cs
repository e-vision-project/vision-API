using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.IO;

public static class CurrentMillis
{
	private static readonly DateTime Jan1St1970 = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>Get extra long current timestamp</summary>
	public static long Millis { get { return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds); } }
}


public class TesseractWrapper_And
{
	[DllImport ("libtess.so")]
	private static extern IntPtr TessVersion ();

	public string Version ()
	{
		IntPtr str_ptr = TessVersion ();
		string tessVersion = Marshal.PtrToStringAnsi (str_ptr);
		return tessVersion;

	}

	IntPtr tessHandle = IntPtr.Zero;

	[DllImport ("libtess.so")]
	private static extern IntPtr TessBaseAPICreate ();

	[DllImport ("libtess.so")]
	private static extern void TessBaseAPIDelete (IntPtr handle);

	[DllImport ("libtess.so")]
	private static extern int TessBaseAPIInit3 (IntPtr handle, string dataPath, string language);

	[DllImport ("libtess.so")]
	private static extern void  TessBaseAPISetImage (IntPtr handle, IntPtr imagedata, int width, int height,
	                                                 int bytes_per_pixel, int bytes_per_line);

	[DllImport ("libtess.so")]
	private static extern void  TessBaseAPISetImage2 (IntPtr handle, IntPtr pix);


	[DllImport ("libtess.so")]
	private static extern IntPtr  TessBaseAPIGetInputImage (IntPtr handle);

	[DllImport ("libtess.so")]
	private static extern IntPtr  TessBaseAPIGetThresholdedImage (IntPtr handle);


	[DllImport ("libtess.so")]
	private static extern int TessBaseAPIRecognize (IntPtr handle, IntPtr monitor);

	[DllImport ("libtess.so")]
	private static extern IntPtr  TessBaseAPIGetUTF8Text (IntPtr handle);

	[DllImport ("libtess.so")]
	private static extern void  TessDeleteText (IntPtr text);

	[DllImport ("libtess.so")]
	private static extern void  TessBaseAPIEnd (IntPtr handle);

	[DllImport ("libtess.so")]
	private static extern void  TessBaseAPIClear (IntPtr handle);


	[DllImport ("liblept.so")]
	private static extern IntPtr pixRead (string fileName);

	[DllImport ("liblept.so")]
	private static extern IntPtr pixDestroy (ref IntPtr ptr);

	[DllImport ("liblept.so")]
	private static extern int pixWritePng (string filename, IntPtr img, float gamma);


	public TesseractWrapper_And ()
	{
		tessHandle = IntPtr.Zero;
	}

	private string tempFName = "tessTemp";
	public string errorMsg;

	public bool Init (string lang, string dataPath)
	{
		if (!tessHandle.Equals (IntPtr.Zero))
			Close ();
		try {
			tessHandle = TessBaseAPICreate ();  
			if (tessHandle.Equals (IntPtr.Zero)) {
				errorMsg = "TessAPICreate failed";
				return false;
			}
			if (dataPath == null) {
				if (!prepareTessdata (false))
					return false;
				dataPath = destPath;
			}
    
			if (TessBaseAPIInit3 (tessHandle, dataPath, lang) != 0) {
				Close ();
				errorMsg = "TessAPIInit failed";
				return false;
			}
			System.Random r = new System.Random (System.DateTime.Now.Millisecond);
			tempFName += r.Next (1000);
			tempFName += ".png";
		} catch (Exception ex) {
			errorMsg = ex.Message;
		}
		return true;
	}

	public void Close ()
	{
		if (tessHandle.Equals (IntPtr.Zero))
			return;
		TessBaseAPIEnd (tessHandle);
		TessBaseAPIDelete (tessHandle);
		tessHandle = IntPtr.Zero;

	}

	public long recognizeRawTime = 0;
	public bool bClearEveryStep = true;

	public string Recognize (Color32[] colors, int width, int height)
	{
		if (tessHandle.Equals (IntPtr.Zero))
			return null;
		int count = width * height;
		int bpp = 4;
		byte[] dataBytes = new byte[count * bpp];
		int bytePtr = 0;
		int colorIdx = count - 1;
		//
		for (int y = height - 1; y >= 0; y--)
			for (int x = 0 /*width - 1*/; x < width; x++) {
				colorIdx = y * width + x;
				dataBytes [bytePtr++] = colors [colorIdx].r;
				dataBytes [bytePtr++] = colors [colorIdx].g;
				dataBytes [bytePtr++] = colors [colorIdx].b;
				dataBytes [bytePtr++] = colors [colorIdx].a;
			}
		//
		//
		IntPtr imgPtr = Marshal.AllocHGlobal (count * bpp);
		Marshal.Copy (dataBytes, 0, imgPtr, count * bpp);
		TessBaseAPISetImage (tessHandle, imgPtr, width, height, bpp, width * bpp);
		recognizeRawTime = CurrentMillis.Millis;
		if (TessBaseAPIRecognize (tessHandle, IntPtr.Zero) != 0) {
			Marshal.FreeHGlobal (imgPtr);
			recognizeRawTime = CurrentMillis.Millis - recognizeRawTime;
			return null;
		}
		recognizeRawTime = CurrentMillis.Millis - recognizeRawTime;
		IntPtr str_ptr = TessBaseAPIGetUTF8Text (tessHandle);
		Marshal.FreeHGlobal (imgPtr);
		if (str_ptr.Equals (IntPtr.Zero))
			return null;
		string tessTextAuto = Marshal.PtrToStringAuto (str_ptr);
		if (bClearEveryStep)
			TessBaseAPIClear (tessHandle);
		TessDeleteText (str_ptr);
		return tessTextAuto;
	}



	public string RecognizeFromTexture (Texture2D texture, bool doCopy)
	{
		Texture2D workTex = texture;
		if (doCopy) {
			workTex = new Texture2D (texture.width, texture.height, TextureFormat.ARGB32, false);
			workTex.SetPixels32 (texture.GetPixels32 ());
			workTex.Apply ();
		}
		return Recognize (workTex.GetPixels32 (), workTex.width, workTex.height);
	}

	public long recognizeFileRawTime = 0;


	public string RecognizeFromFile (string filePath, int procCount)
	{
		if (tessHandle.Equals (IntPtr.Zero))
			return null;
		IntPtr imgPtr = pixRead (filePath);
		if (imgPtr.Equals (IntPtr.Zero))
			return null;
		TessBaseAPISetImage2 (tessHandle, imgPtr);
		recognizeFileRawTime = CurrentMillis.Millis;
		if (TessBaseAPIRecognize (tessHandle, IntPtr.Zero) != 0) {
			pixDestroy (ref imgPtr);
			recognizeFileRawTime = CurrentMillis.Millis - recognizeFileRawTime;
			return null;
		}
		recognizeFileRawTime = CurrentMillis.Millis - recognizeFileRawTime;
		IntPtr str_ptr = TessBaseAPIGetUTF8Text (tessHandle);
		if (str_ptr.Equals (IntPtr.Zero))
			return null;
		string tessTextAuto = Marshal.PtrToStringAuto (str_ptr);
		if (bClearEveryStep)
			TessBaseAPIClear (tessHandle);
		pixDestroy (ref imgPtr);
		TessDeleteText (str_ptr);
		return tessTextAuto;
	}

	///   tessdata prepare part
	public static int EXTRACT_TESSDATAFILE_TIMEOUT_MSEC = 300000;

	private bool extractFile (string fileName, string srcFilePath, string destFilePath)
	{
		try {
			string fullpath = srcFilePath + fileName;
			WWW loadFile = new WWW (fullpath);
			long startTime = CurrentMillis.Millis;
			while (!loadFile.isDone) { 
				long elapsed = CurrentMillis.Millis - startTime; 
				if (elapsed > EXTRACT_TESSDATAFILE_TIMEOUT_MSEC)
					return false;
			}  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
			fullpath = destFilePath + fileName;
			File.WriteAllBytes (fullpath, loadFile.bytes);
		} catch (Exception ex) {
			errorMsg = "Extract file " + fileName + " failed:" + ex.Message;
			return false;
		}
		return  true;
	}

	string destPath;
	static string tessPath = "/tesseract/tessdata/";
	static string[] delim = { "\r\n" };

	private bool prepareTessdata (bool bForce)
	{
		string filelistname = "filelist.txt"; 
		destPath = Application.persistentDataPath + tessPath; 
		string filepath = destPath + filelistname;
		string filelist;
		string[] files;
		if (File.Exists (filepath) && !bForce) {
			// check integrity
			filelist = File.ReadAllText (filepath);
			if (filelist.Length > 0) {
				files = filelist.Split (delim, StringSplitOptions.None);
				bool bOk = true;
				foreach (string filename in files) {
					if (filename.Length > 3 && !File.Exists (destPath + filename))
						bOk = false;
					break;
				}
				if (bOk)
					return true;
			}
		}
		//
		string dirname = Application.persistentDataPath + "/tesseract";
		Directory.CreateDirectory (dirname);
		if (!Directory.Exists (dirname)) {
			errorMsg = "Failed to create " + dirname;
			return false;     
		}
		dirname = dirname + "/tessdata";
		Directory.CreateDirectory (dirname);
		if (!Directory.Exists (dirname)) {
			errorMsg = "Failed to create " + dirname;
			return false;
		}
		string srcPath = Application.streamingAssetsPath + tessPath;
		filepath = srcPath + filelistname;
		WWW loadFile = new WWW (filepath);
		long startTime = CurrentMillis.Millis;
		while (!loadFile.isDone) { 
			long elapsed = CurrentMillis.Millis - startTime; 
			if (elapsed > 10000)
				break;
		}  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check

		string errStr = loadFile.error;
		if (errStr != null && errStr != string.Empty) {
			errorMsg = "Get filelist.txt failed:" + errStr;
			return false;
		}
		filelist = loadFile.text;
		files = filelist.Split (delim, StringSplitOptions.None);
		long extractTime = CurrentMillis.Millis;
		bool bExtracted = true;
		foreach (string filename in files) {
			if (filename.Length > 3)
				bExtracted &= extractFile (filename, srcPath, destPath);
		}
		filepath = destPath + filelistname;
		File.WriteAllText (filepath, filelist);
		extractTime = CurrentMillis.Millis - extractTime;
		return  bExtracted;

	}
}

