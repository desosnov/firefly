/*
 * PointDistancePrimitive.h
 *
 *  Created on: Aug 4, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_PRIMITIVES_BASE_POINTDISTANCEPRIMITIVE_H_
#define ANIMATION_PRIMITIVES_BASE_POINTDISTANCEPRIMITIVE_H_

#include <animation/base/APrimitive.h>

class PointDistancePrimitive: public virtual APrimitive {
protected:
	glm::vec4 renderPixelAt(glm::vec3 pos, ArbitraryMap* details);
	virtual glm::vec4 renderAtDistanceFromPoint(double distance, ArbitraryMap* details) =0;

public:
	glm::vec3 centerPos = glm::vec3(0.0, 0.0, 0.0);

	PointDistancePrimitive(glm::vec3 centerPoint);
	virtual ~PointDistancePrimitive();


};

#endif /* ANIMATION_PRIMITIVES_BASE_POINTDISTANCEPRIMITIVE_H_ */
