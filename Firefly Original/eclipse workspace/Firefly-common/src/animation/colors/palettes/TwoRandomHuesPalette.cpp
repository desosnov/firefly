/*
 * TwoRandomHuesPalette.cpp
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#include <animation/colors/HSVRGB.h>
#include <animation/colors/palettes/TwoRandomHuesPalette.h>
#include "FireflyUtils.h"

TwoRandomHuesPalette::TwoRandomHuesPalette() {
	randomizeHues();
}

TwoRandomHuesPalette::~TwoRandomHuesPalette() {
	// TODO Auto-generated destructor stub
}


void TwoRandomHuesPalette::randomizeHues() {
	hue1 = rand(0.0, 360.0);
	do {
		hue2 = rand(0.0, 360.0);
	} while (fabs(hue1 - hue2) < TRH_MIN_HUE_DISTANCE);
}

glm::vec4 TwoRandomHuesPalette::nextColor() {
	glm::vec4 color;

	float hue = rand1() > 0.5 ? hue1 : hue2;
	float sat = rand(TRH_MIN_SATURATION, TRH_MAX_SATURATION);
	float val = 1.0;
	HSVtoRGB(color.r, color.g, color.b, hue, sat, val);
	color.a = 1.0;

	checkAgainstLastColor(color);
	return color;
}
