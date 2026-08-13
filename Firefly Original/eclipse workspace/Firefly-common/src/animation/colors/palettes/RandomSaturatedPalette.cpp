/*
* RandomChromaticColors.cpp
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#include <animation/colors/palettes/RandomSaturatedPalette.h>

RandomSaturatedPalette::~RandomSaturatedPalette() {
	// TODO Auto-generated destructor stub
}

void RandomSaturatedPalette::setMinSaturation(double sat) {
	saturation = sat;
}

glm::vec4 RandomSaturatedPalette::nextColor() {
	glm::vec4 newColor;

	float hue = rand(0.0,360.0);
	float sat = rand(saturation, 1.0);
	float val = 1.0;

	HSVtoRGB(newColor.r, newColor.g, newColor.b, hue, sat, val);
	newColor.a = 1.0;

	checkAgainstLastColor(newColor);
	return newColor;
}
