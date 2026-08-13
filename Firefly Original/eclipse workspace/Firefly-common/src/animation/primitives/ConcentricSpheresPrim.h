/*
 * ConcentricSpheresPrim.h
 *
 *  Created on: Aug 6, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_PRIMITIVES_CONCENTRICSPHERESPRIM_H_
#define ANIMATION_PRIMITIVES_CONCENTRICSPHERESPRIM_H_

#include <animation/primitives/base/PointDistancePrimitive.h>
#include <animation/transform/IntervalSlicerWithSymmetricalPhases.h>
#include <vector>

#define CSP_MIN_ALPHA 0.85
#define CSP_EDGE_FADE_SIZE 3.0

class ConcentricSpheresPrim : public PointDistancePrimitive {
protected:
	glm::vec4 renderAtDistanceFromPoint(double distance, ArbitraryMap* details);
	IntervalSlicerWithSymmetricalPhases slicer;

public:
	std::vector<glm::vec4> colors;
	double colorBlendingFactor;
	double ringRadius, sizeInRings;
	int finalRing = INT_MAX;

	ConcentricSpheresPrim(
			glm::vec3 centerPoint = glm::vec3(0.0, 0.0, 0.0),
			double ringRadius = 1.0,
			double sizeInRings = 10.0,
			std::vector<glm::vec4> colors = std::vector<glm::vec4>({glm::vec4(1.0, 1.0, 1.0, 1.0)}),
			double colorBlendingFactor = 0.0)
			: PointDistancePrimitive(centerPoint),
			  ringRadius(ringRadius),
			  sizeInRings(sizeInRings),
			  colors(colors),
			  colorBlendingFactor(colorBlendingFactor),
			  slicer(0.0, ringRadius)
	{};
	virtual ~ConcentricSpheresPrim();


};

#endif /* ANIMATION_PRIMITIVES_CONCENTRICSPHERESPRIM_H_ */
