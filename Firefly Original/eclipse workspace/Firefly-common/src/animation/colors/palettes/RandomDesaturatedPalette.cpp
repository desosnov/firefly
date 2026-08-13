/*
 * RandomDesaturatedPalette.cpp
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#include <animation/colors/HSVRGB.h>
#include <animation/colors/palettes/RandomDesaturatedPalette.h>
#include "FireflyUtils.h"

RandomDesaturatedPalette::~RandomDesaturatedPalette() {
	// TODO Auto-generated destructor stub
}

void RandomDesaturatedPalette::setMaxSaturation(double sat) {
	maxSaturation = sat;
}

glm::vec4 RandomDesaturatedPalette::nextColor() {
	glm::vec4 newColor;

	float hue = rand(0.0, 360.0);
	float sat = rand(RDP_MIN_SATURATION, maxSaturation);
	float val = 1.0;

	HSVtoRGB(newColor.r, newColor.g, newColor.b, hue, sat, val);
	newColor.a = 1.0;

	checkAgainstLastColor(newColor);
	return newColor;
}
