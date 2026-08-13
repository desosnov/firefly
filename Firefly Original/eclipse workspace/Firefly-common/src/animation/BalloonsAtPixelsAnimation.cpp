/*
 * BalloonsAtPixelsAnimation.cpp
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#include <animation/BalloonsAtPixelsAnimation.h>

BalloonsAtPixelsAnimation::~BalloonsAtPixelsAnimation() {
	// TODO Auto-generated destructor stub
}

SpherePrim* BalloonsAtPixelsAnimation::newSphere() {

	int pixel = rand() % stage->pixelsLen;
	glm::vec3 center = stage->pixels[pixel].getPos();
	glm::vec3 offset = glm::vec3(rand(-1.0, 1.0), rand(-1.0, 1.0), rand(-1.0, 1.0)) * (float)stage->getPixelRadius();
	center += offset;

	SpherePrim* sphere = new SpherePrim(center, 0.0, colorScheme->nextColor(), numSpheres);
	numSpheres++;
	return sphere;
}
