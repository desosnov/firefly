/*
 * EasingFunction1D.cpp
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#include <animation/base/AEasingFunction1D.h>


AEasingFunction1D::~AEasingFunction1D() {
}

void AEasingFunction1D::bindValue(double* valPtr) {
	if(std::find(bindings.begin(), bindings.end(), valPtr) == bindings.end()) {
		bindings.push_back(valPtr);
	}
}

void AEasingFunction1D::update(double time) {
	for(std::vector<double*>::iterator iter = bindings.begin(); iter != bindings.end(); iter++) {
		**iter = easedValue(time);
	}

	if((time - start) / (finish - start) > 1.0) {
		finishedFlag = true;
	} else {
		finishedFlag = false;
	}
}

double AEasingFunction1D::normalizeInput(double input) {
	double normalized = (input - start) / (finish - start);

	return fmax(fmin(normalized, 1.0), 0.0);
}

bool AEasingFunction1D::finished() {
	return finishedFlag;
}
