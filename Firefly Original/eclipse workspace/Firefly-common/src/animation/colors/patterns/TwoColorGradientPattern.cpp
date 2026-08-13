/*
 * TwoColorGradientPattern.cpp
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#include <animation/colors/patterns/TwoColorGradientPattern.h>

TwoColorGradientPattern::~TwoColorGradientPattern() {
	// TODO Auto-generated destructor stub
}

glm::vec4 TwoColorGradientPattern::getColor() {
	return color;
}

glm::vec4 TwoColorGradientPattern::getColor(double x) {
	return (float)(1.0-x)*color + (float)x*color2;
}
