/*
 * CosineEase4D.cpp
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#include <animation/easing/CosineEase4D.h>

CosineEase4D::~CosineEase4D() {
	// TODO Auto-generated destructor stub
}

glm::vec4 CosineEase4D::easedValue(double input) {
	double norm = normalizeInput(input);

	return easeFrom + (easeTo-easeFrom) * (float)(-0.5*cos(norm*M_PI) + 0.5);
}
