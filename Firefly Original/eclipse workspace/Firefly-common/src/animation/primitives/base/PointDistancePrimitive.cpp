/*
 * PointDistancePrimitive.cpp
 *
 *  Created on: Aug 4, 2016
 *      Author: Denis
 */

#include <animation/primitives/base/PointDistancePrimitive.h>

PointDistancePrimitive::PointDistancePrimitive(glm::vec3 centerPoint) {
	centerPos = centerPoint;
}

PointDistancePrimitive::~PointDistancePrimitive() {
	// TODO Auto-generated destructor stub
}

glm::vec4 PointDistancePrimitive::renderPixelAt(glm::vec3 pos, ArbitraryMap* details) {
	return renderAtDistanceFromPoint(glm::distance(pos, centerPos), details);
}
