//
//  iOSController.m
//  Tesseract_iOS_test
//
//  Created by Aleksandr Suchkov NTR-Lab on 30.05.17.
//  Copyright © 2017 Aleksandr Suchkov. All rights reserved.
//

#import "iOSController.h"

@interface TesseractObject ()

@end

@implementation TesseractObject : G8Tesseract

- (instancetype)initPrivateWithLanguage: (const char*) language
{
    self = [super initWithLanguage:@"language"];
    return self;
}

+ (instancetype)sharedInstance: (char*) language remove: (BOOL*) remove
{
    
    static dispatch_once_t onceToken;
    static NSMutableDictionary  *uniqueInstance = nil;
    
    dispatch_once(&onceToken, ^{
        uniqueInstance = [[NSMutableDictionary alloc] init];
    });
    
    if (!language)
        return nil;
    
    NSString *languageString = [NSString stringWithUTF8String:language];
    
    if (remove) {
        [uniqueInstance removeObjectForKey:languageString];
        return nil;
    }
    
    TesseractObject *objectForReturn =  uniqueInstance[languageString];
    if (objectForReturn)
        return objectForReturn;
    
    NSString *path = [[[NSBundle mainBundle] resourcePath] stringByAppendingString: @"/Data/Raw/"];
    TesseractObject *newObject = [[TesseractObject alloc] initWithLanguage: languageString configDictionary: nil configFileNames: nil absoluteDataPath:path engineMode: G8OCREngineModeTesseractOnly];
    uniqueInstance[languageString] = newObject;
    
    NSLog(@"Tesseract objects %lu", (unsigned long)uniqueInstance.count);
    
    return uniqueInstance[languageString];
}
@end

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

void *TessBaseAPICreate_iOS (char* dataPath, char* lang){
    
    G8Tesseract *tesseract = [TesseractObject sharedInstance: lang remove:NO];
    
}

void TessBaseAPIDelete_iOS (char* lang){
    
    [TesseractObject sharedInstance: lang remove: YES];
    
}

const char* TesseractOCRVersion_iOS ()
{
    return MakeStringCopy([[G8Tesseract version] UTF8String]);
}


char* MakeTesseractPhoto_iOS ( char* lang, char* charEncodeData) {
    
    G8Tesseract *tesseract = [TesseractObject sharedInstance: lang remove: NO];
    
    NSString *filePath = [NSString stringWithFormat:@"%s", charEncodeData];
    if  ( ![[NSFileManager defaultManager] fileExistsAtPath:[NSString stringWithFormat:@"%s", charEncodeData]]) {
        return "";
    }
    
    UIImage* image = [UIImage imageWithContentsOfFile:filePath];
    tesseract.image =  image;
    
    tesseract.maximumRecognitionTime = 2.0;
    
    [tesseract recognize];
    
    // Retrieve the recognized text
    NSLog(@"Tesseract result %@", [tesseract recognizedText]);
    
    return MakeStringCopy([[tesseract recognizedText] UTF8String]);
}

char* MakeTesseractPhotoFromMemory_iOS ( char* lang, char* charEncodeData) {
    
    NSString *base64String = [NSString stringWithFormat:@"%s", charEncodeData];
    
    NSData* data = [[NSData alloc] initWithBase64EncodedString:base64String options:0];
    UIImage *image = [UIImage imageWithData:data];
    
    G8Tesseract *tesseract = [TesseractObject sharedInstance: lang remove: NO];
    
    tesseract.image =  image;
    
    tesseract.maximumRecognitionTime = 2.0;
    
    [tesseract recognize];
    
    // Retrieve the recognized text
    NSLog(@"Tesseract result %@", [tesseract recognizedText]);
    
    return MakeStringCopy([[tesseract recognizedText] UTF8String]);
    
}




