/*
 * GradientToTransparentScheme.cpp
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#include <animation/colors/patterns/TwoColorGradientPattern.h>
#include <animation/colors/schemes/GradientToTransparentScheme.h>

GradientToTransparentScheme::~GradientToTransparentScheme() {
	// TODO Auto-generated destructor stub
}

AColorPattern* GradientToTransparentScheme::nextColor() {
	glm::vec4 color = palette->randomColor();
	glm::vec4 color2 = color;
	color2.a = 0.0;

	return new TwoColorGradientPattern(color, color2);
}
