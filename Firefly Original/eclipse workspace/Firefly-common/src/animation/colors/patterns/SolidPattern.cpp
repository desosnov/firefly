/*
 * SolidColorPattern.cpp
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#include <animation/colors/patterns/SolidPattern.h>

SolidPattern::~SolidPattern() {
	// TODO Auto-generated destructor stub
}

glm::vec4 SolidPattern::getColor() {
	return color;
}
