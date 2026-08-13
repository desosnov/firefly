/*
 * SingleRandomHuePalette.cpp
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#include <animation/colors/HSVRGB.h>
#include <animation/colors/palettes/SingleRandomHuePalette.h>
#include "FireflyUtils.h"

SingleRandomHuePalette::SingleRandomHuePalette() {
	randomizeHue();
}

SingleRandomHuePalette::~SingleRandomHuePalette() {
	// TODO Auto-generated destructor stub
}

void SingleRandomHuePalette::randomizeHue() {
	hue = rand(0.0, 360.0);
}

glm::vec4 SingleRandomHuePalette::nextColor() {
	glm::vec4 color;

	float sat = rand(SRH_MIN_SATURATION, SRH_MAX_SATURATION);
	float val = 1.0;
	HSVtoRGB(color.r, color.g, color.b, hue, sat, val);
	color.a = 1.0;

	checkAgainstLastColor(color);
	return color;
}
