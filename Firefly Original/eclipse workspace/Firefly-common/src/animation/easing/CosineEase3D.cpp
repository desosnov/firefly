/*
 * CosineEase3D.cpp
 *
 *  Created on: Aug 8, 2016
 *      Author: Denis
 */

#include <animation/easing/CosineEase3D.h>

CosineEase3D::~CosineEase3D() {
	// TODO Auto-generated destructor stub
}

glm::vec3 CosineEase3D::easedValue(double input) {
	double norm = normalizeInput(input);

	return easeFrom + (easeTo-easeFrom) * (float)(-0.5*cos(norm*M_PI) + 0.5);
}
