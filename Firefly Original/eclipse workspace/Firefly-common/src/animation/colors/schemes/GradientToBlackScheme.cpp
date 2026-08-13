/*
 * GradientToBlackScheme.cpp
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#include <animation/colors/schemes/GradientToBlackScheme.h>

GradientToBlackScheme::~GradientToBlackScheme() {
	// TODO Auto-generated destructor stub
}

AColorPattern* GradientToBlackScheme::nextColor() {
	return new TwoColorGradientPattern(palette->randomColor(), glm::vec4(0.0, 0.0, 0.0, 1.0));
}

