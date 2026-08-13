/*
 * SolidColorsScheme.cpp
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#include <animation/colors/schemes/SolidColorsScheme.h>

SolidColorsScheme::~SolidColorsScheme() {
	// TODO Auto-generated destructor stub
}

AColorPattern* SolidColorsScheme::nextColor() {
	return new SolidPattern(palette->nextColor());
}
