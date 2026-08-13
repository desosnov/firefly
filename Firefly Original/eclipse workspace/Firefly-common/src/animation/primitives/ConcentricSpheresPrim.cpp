/*
 * ConcentricSpheresPrim.cpp
 *
 *  Created on: Aug 6, 2016
 *      Author: Denis
 */

#include <animation/primitives/ConcentricSpheresPrim.h>

glm::vec4 ConcentricSpheresPrim::renderAtDistanceFromPoint(double distance, ArbitraryMap* details) {
	double distanceIntoSphere = sizeInRings * ringRadius - distance;

	slicer.interval = ringRadius;
	int ring = slicer.getInterval(distanceIntoSphere);
	double phase = slicer.getPhase(distanceIntoSphere);

	int colorIndex = ring % colors.size();
	int blendColorIndex = phase <= 0.5 ? (ring-1) % colors.size() : (ring+1) % colors.size();

	float colorBrightness = fmax(0.0, slicer.getSymmetricalPhase(distanceIntoSphere) + colorBlendingFactor) / (1.0 + colorBlendingFactor);
	float blendColorBrightness = fmax(0.0,colorBlendingFactor - slicer.getSymmetricalPhase(distanceIntoSphere)) / (1.0 + colorBlendingFactor);

	glm::vec4 color;
	if(ring < 0 || ring > finalRing) {
		color = glm::vec4(0.0, 0.0, 0.0, 0.0);
	} else if ((ring == 0 && phase < 0.5) || (ring == finalRing && phase > 0.5)) {
		color = colors[colorIndex];
		color.a = fmin(colorBrightness, (ring == 0 ? phase : 1.0-phase)/0.5);
	} else {
		color = colors[colorIndex]*(1.0f-blendColorBrightness) + colors[blendColorIndex]*blendColorBrightness;
		color *= colorBrightness + blendColorBrightness;
		color += glm::vec4(0.0, 0.0, 0.0, CSP_MIN_ALPHA) * (1.0f - colorBrightness - blendColorBrightness);
	}

	float distToEdge = fmin((float)ring + phase, (float)finalRing+1.0-(float)ring-phase);
	float edgeAlphaMultiplier = fmax(0.0, fmin(1.0, distToEdge/CSP_EDGE_FADE_SIZE));
	color.a *= edgeAlphaMultiplier;

	if(colorBrightness > 0.0 && ring >= 0) {
		details->setInt("shaderIndex", ring);
	}
	return color;
}

ConcentricSpheresPrim::~ConcentricSpheresPrim() {
	// TODO Auto-generated destructor stub
}

