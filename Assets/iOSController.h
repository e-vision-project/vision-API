//
//  iOSController.h
//  Tesseract_iOS_test
//
//  Created by Aleksandr Suchkov NTR-Lab on 30.05.17.
//  Copyright © 2017 Aleksandr Suchkov. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <TesseractOCR/TesseractOCR.h>

@interface TesseractObject : G8Tesseract <G8TesseractDelegate>

- (instancetype)init UNAVAILABLE_ATTRIBUTE;
+ (instancetype)sharedInstance: ( char *) language remove: (BOOL*) remove;

@end

