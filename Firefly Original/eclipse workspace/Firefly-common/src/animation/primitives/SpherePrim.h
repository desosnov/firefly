/*
 * SpherePrim.h
 *
 *  Created on: Aug 9, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_PRIMITIVES_SPHEREPRIM_H_
#define ANIMATION_PRIMITIVES_SPHEREPRIM_H_

#include <animation/colors/patterns/SolidPattern.h>
#include <animation/primitives/base/PointDistancePrimitive.h>

#define SP_SOFT_EDGE_RATIO 0.05

class SpherePrim: public PointDistancePrimitive {
protected:
	glm::vec4 renderAtDistanceFromPoint(double distance, ArbitraryMap* details);

public:
	AColorPattern* colorPattern;
	double radius;
	int shaderIndex = -1;

	SpherePrim(
			glm::vec3 centerPoint = glm::vec3(0.0, 0.0, 0.0),
			double radius = 1.0,
			AColorPattern* colorPattern = new SolidPattern(),
			int shaderIndex = -1)
			: PointDistancePrimitive(centerPoint),
			  radius(radius),
			  colorPattern(colorPattern),
			  shaderIndex(shaderIndex)
	{};
	virtual ~SpherePrim();
};

#endif /* ANIMATION_PRIMITIVES_SPHEREPRIM_H_ */
