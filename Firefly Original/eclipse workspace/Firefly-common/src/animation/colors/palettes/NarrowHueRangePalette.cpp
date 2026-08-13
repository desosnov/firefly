/*
 * RandomHueRangePalette.cpp
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#include <animation/colors/HSVRGB.h>
#include <animation/colors/palettes/NarrowHueRangePalette.h>
#include "FireflyUtils.h"

NarrowHueRangePalette::NarrowHueRangePalette() {
	randomizeHue();

}

NarrowHueRangePalette::~NarrowHueRangePalette() {
	// TODO Auto-generated destructor stub
}

void NarrowHueRangePalette::randomizeHue() {
	minHue = rand(0.0, 360.0);
	maxHue = minHue + rand(NHR_MIN_HUE_RANGE, NHR_MAX_HUE_RANGE);
}

glm::vec4 NarrowHueRangePalette::nextColor() {
	glm::vec4 color;

	float hue = fmod((float)rand(minHue, maxHue), 360.0f);
	float sat = rand(NHR_MIN_SATURATION, NHR_MAX_SATURATION);
	float val = 1.0;
	HSVtoRGB(color.r, color.g, color.b, hue, sat, val);
	color.a = 1.0;

	checkAgainstLastColor(color);
	return color;
}


