/*
 * WideHueRangePalette.cpp
 *
 *  Created on: Sep 25, 2016
 *      Author: d
 */

#include <animation/colors/palettes/WideHueRangePalette.h>
#include <animation/colors/HSVRGB.h>
#include "FireflyUtils.h"

WideHueRangePalette::WideHueRangePalette() {
	randomizeHue();
}

WideHueRangePalette::~WideHueRangePalette() {
	// TODO Auto-generated destructor stub
}

void WideHueRangePalette::randomizeHue() {
	minHue = rand(0.0, 360.0);
	maxHue = minHue + rand(WHR_MIN_HUE_RANGE, WHR_MAX_HUE_RANGE);
}

glm::vec4 WideHueRangePalette::nextColor() {
	glm::vec4 color;

	float hue = fmod((float)rand(minHue, maxHue), 360.0f);
	float sat = rand(WHR_MIN_SATURATION, WHR_MAX_SATURATION);
	float val = 1.0;
	HSVtoRGB(color.r, color.g, color.b, hue, sat, val);
	color.a = 1.0;

	checkAgainstLastColor(color);
	return color;
}


