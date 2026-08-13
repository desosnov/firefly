/*
 * ColorScheme.cpp
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#include <animation/base/AColorScheme.h>

AColorScheme::~AColorScheme() {
	// TODO Auto-generated destructor stub
}

void AColorScheme::setPalette(AColorPalette* palette) {
	this->palette = palette;
}

AColorPalette* AColorScheme::getPalette() {
	return palette;
}
