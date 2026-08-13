/*
 * AColorPattern.cpp
 *
 *  Created on: Aug 24, 2016
 *      Author: d
 */

#include <animation/base/AColorPattern.h>

AColorPattern::~AColorPattern() {
	// TODO Auto-generated destructor stub
}

glm::vec4 AColorPattern::getColor(double x) {
	return getColor();
}

glm::vec4 AColorPattern::getColor(double x, double y) {
	return getColor(x);
}

glm::vec4 AColorPattern::getColor(glm::vec3 pos) {
	return getColor();
}

glm::vec4 AColorPattern::getColor(double x, glm::vec3 pos) {
	if(getColor(x) != getColor()) {
		return getColor(x);
	}
	return getColor(pos);
}

glm::vec4 AColorPattern::getColor(double x, double y, glm::vec3 pos) {
	if(getColor(x,y) != getColor()) {
		return getColor(x,y);
	}
	return getColor(pos);
}
