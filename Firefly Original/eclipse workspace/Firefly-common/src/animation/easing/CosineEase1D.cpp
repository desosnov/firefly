/*
 * CosineEase1D.cpp
 *
 *  Created on: Aug 8, 2016
 *      Author: Denis
 */

#include <animation/easing/CosineEase1D.h>
#include "FireflyUtils.h"

CosineEase1D::~CosineEase1D() {
	// TODO Auto-generated destructor stub
}

double CosineEase1D::easedValue(double input) {
	double norm = normalizeInput(input);

	return easeFrom + (easeTo-easeFrom) * (-0.5*cos(norm*M_PI) + 0.5);
}
